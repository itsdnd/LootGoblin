using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LootGoblin.Storage;
using LootGoblin.Storage.Trees;
using LootGoblin.Storage.Grids;
using System.Linq;

namespace LootGoblin.Controls
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        private ProgramStorage programStorage;
        private List<string> magicItemTypes;
        private List<string> magicItemRarities;

        private const int Number_Maximum = 1000;

        public MainControl()
        {
            InitializeComponent();
            programStorage = ProgramStorage.GetInstance();

            SplashScreen splashScreen = new SplashScreen(this);
            splashScreen.Show();
        }

        public void Initialize()
        {
            magicItemTypes = new List<string>();
            magicItemRarities = new List<string>();

            // Load everything
            GenerateContainerTypeList();
            PopulateContainerTree();

            programStorage.GenerateMagicTypesAndRaritiesLists();
            PopulateMagicItemTree();
            PopulateRandomMagicItemControls();

            // Set data sources
            dataEncounterContainers.ItemsSource = programStorage.EncounterList;
            dataMagicItems.ItemsSource = programStorage.MagicItemList;
            dataRandomMagicItems.ItemsSource = programStorage.RandomMagicItemList;

            // Show Window
            Application.Current.MainWindow.Visibility = Visibility.Visible;
        }

        //================================================================================
        // Container Management
        //================================================================================

        private void GenerateContainerTypeList()
        {
            programStorage.ContainerTypes = new List<string>(programStorage.BasicContainerTypes);

            foreach (LootContainer container in programStorage.LootContainers)
            {
                if (!programStorage.ContainerTypes.Contains(container.Type))
                {
                    programStorage.ContainerTypes.Add(container.Type);
                }
            }

            programStorage.ContainerTypes.Sort();
        }

        private void PopulateContainerTree()
        {
            List<ContainerTreeType> treeTypes = new List<ContainerTreeType>();
            foreach (string containerType in programStorage.ContainerTypes)
            {
                ContainerTreeType type = new ContainerTreeType { Name = containerType };

                foreach (LootContainer container in programStorage.LootContainers.OrderBy(x => x.Name))
                {
                    if (container.Type.Equals(containerType, StringComparison.CurrentCultureIgnoreCase))
                    {
                        ContainerTreeName containerName = new ContainerTreeName { Name = container.Name };
                        type.NameList.Add(containerName);
                    }
                }

                if (type.NameList.Count > 0)
                {
                    treeTypes.Add(type);
                }
            }

            containerTree.DataContext = treeTypes;
        }

        private void txtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtQuantity.Text.Length < 1)
            {
                txtQuantity.Text = "1";
                return;
            }

            var value = int.Parse(txtQuantity.Text);
            if (value > Number_Maximum)
            {
                txtQuantity.Text = Number_Maximum.ToString();
            }

            if (value < 1)
            {
                txtQuantity.Text = "1";
            }
        }

        private void txtQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");

            var value = 0;
            var handled = (int.TryParse(e.Text, out value) && regex.IsMatch(e.Text)) ? false : true;

            if (txtQuantity.Text.Length >= 4 && txtQuantity.SelectionLength < 1)
            {
                handled = true;
            }

            e.Handled = handled;
        }

        private void txtQuantity_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        private void btnQuantityUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtQuantity.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtQuantity.Text = value.ToString();
        }

        private void btnQuantityDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtQuantity.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtQuantity.Text = value.ToString();
        }

        private void btnAddToEncounter_Click(object sender, RoutedEventArgs e)
        {
            if (containerTree.SelectedItem == null || programStorage.ContainerTypes.Contains(containerTree.SelectedValue.ToString()))
            {
                return;
            }

            var selection = containerTree.SelectedValue.ToString();
            EncounterContainer encounterContainer = new EncounterContainer(int.Parse(txtQuantity.Text), selection);
            programStorage.EncounterList.Add(encounterContainer);

            dataEncounterContainers.Items.Refresh();
        }

        private void btnEditContainers_Click(object sender, RoutedEventArgs e)
        {
            ContainerWindow containerWindow = new ContainerWindow();
            containerWindow.ShowDialog();

            RemoveDeletedEncounterContainers();
            PopulateContainerTree();
        }

        private void RemoveDeletedEncounterContainers()
        {
            List<EncounterContainer> remove = new List<EncounterContainer>();
            foreach (EncounterContainer encounterContainer in programStorage.EncounterList)
            {
                var found = false;
                foreach (LootContainer container in programStorage.LootContainers)
                {
                    if (encounterContainer.Name.Equals(container.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    remove.Add(encounterContainer);
                }
            }

            foreach (EncounterContainer removeContainer in remove)
            {
                programStorage.EncounterList.Remove(removeContainer);
            }
        }

        private void containerTree_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (programStorage.Settings.OverrideContainerListMouseWheelScrolling)
            {
                ((TreeView)sender).CaptureMouse();
            }
        }

        private void containerTree_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (programStorage.Settings.OverrideContainerListMouseWheelScrolling)
            {
                ((TreeView)sender).ReleaseMouseCapture();
            }
        }

        //================================================================================
        // Encounter - Container List
        //================================================================================

        private void btnEncounterDuplicate_Click(object sender, RoutedEventArgs e)
        {
            foreach (EncounterContainer selection in dataEncounterContainers.SelectedItems)
            {
                EncounterContainer encounterContainer = new EncounterContainer(selection.Quantity, selection.Name);
                programStorage.EncounterList.Add(encounterContainer);
            }
            dataEncounterContainers.Items.Refresh();
        }

        private void btnEncounterRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataEncounterContainers.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressEncounterListPopups)
            {
                var proceed = WarningPopup.Show("Remove Selection?", "Are you sure you want to remove the selected container(s)?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var selections = dataEncounterContainers.SelectedItems;
            for (var i = programStorage.EncounterList.Count - 1; i >= 0; i--)
            {
                if (selections.Contains(programStorage.EncounterList[i]))
                {
                    programStorage.EncounterList.RemoveAt(i);
                }
            }

            dataEncounterContainers.Items.Refresh();
        }

        private void btnEncounterClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataEncounterContainers.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressEncounterListPopups)
            {
                var proceed = WarningPopup.Show("Clear Containers?", "Are you sure you want to clear the container list?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            programStorage.EncounterList.Clear();
            dataEncounterContainers.Items.Refresh();
        }

        private void btnLoadEncounter_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();

            string encounterPath = Path.Combine(Environment.CurrentDirectory, @"encounters\");
            Directory.CreateDirectory(encounterPath);

            openDialog.InitialDirectory = encounterPath;
            openDialog.RestoreDirectory = true;

            if (openDialog.ShowDialog() == true)
            {
                string[] lines = File.ReadAllLines(openDialog.FileName);

                foreach (string line in lines)
                {
                    string[] split = line.Split(',');

                    // Encounter Container
                    if (split.Length == 2)
                    {
                        int quantity = 0;
                        if (int.TryParse(split[0], out quantity) && quantity > 0)
                        {
                            foreach (LootContainer container in programStorage.LootContainers)
                            {
                                if (container.Name.Equals(split[1], StringComparison.CurrentCultureIgnoreCase))
                                {
                                    EncounterContainer encounterContainer = new EncounterContainer(quantity, split[1]);
                                    programStorage.EncounterList.Add(encounterContainer);
                                    break;
                                }
                            }
                        }
                    }

                    // Magic Items
                    if (split.Length == 3)
                    {
                        foreach (MagicItem item in programStorage.MagicItems)
                        {
                            if (item.Name.Equals(split[0], StringComparison.CurrentCultureIgnoreCase) && 
                                item.Type.Equals(split[1], StringComparison.CurrentCultureIgnoreCase) &&
                                item.Rarity.Equals(split[2], StringComparison.CurrentCultureIgnoreCase))
                            {
                                programStorage.MagicItemList.Add(item);
                                break;
                            }
                        }
                    }

                    // Random Magic Items
                    if (split.Length == 4)
                    {
                        foreach (MagicItem item in programStorage.MagicItems)
                        {
                            if (item.Type.Equals(split[0], StringComparison.CurrentCultureIgnoreCase) &&
                                item.Rarity.Equals(split[1], StringComparison.CurrentCultureIgnoreCase)) 
                            {
                                var min = 0;
                                int.TryParse(split[2], out min);

                                var max = 0;
                                int.TryParse(split[3], out max);

                                RandomMagicItem randomItem = new RandomMagicItem(item.Type, item.Rarity, min, max);
                                programStorage.RandomMagicItemList.Add(randomItem);
                                break;
                            }
                        }
                    }
                }

                dataEncounterContainers.Items.Refresh();
            }
        }

        private void btnSaveEncounter_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();

            string encounterPath = Path.Combine(Environment.CurrentDirectory, @"encounters\");
            Directory.CreateDirectory(encounterPath);

            saveDialog.InitialDirectory = encounterPath;
            saveDialog.RestoreDirectory = true;
            saveDialog.FileName = "encounter.txt";
            saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (saveDialog.ShowDialog() == true)
            {
                string output = String.Empty;
                foreach (EncounterContainer container in dataEncounterContainers.Items)
                {
                    output += String.Format("{0},{1}{2}", container.Quantity, container.Name, Environment.NewLine);
                }

                if (checkIncludeMagicItems.IsChecked.Value)
                {
                    output += Environment.NewLine;
                    foreach (MagicItem item in dataMagicItems.Items)
                    {
                        output += String.Format("{0},{1},{2}{3}", item.Name, item.Type, item.Rarity, Environment.NewLine);
                    }

                    output += Environment.NewLine;
                    foreach (RandomMagicItem item in dataRandomMagicItems.Items)
                    {
                        output += String.Format("{0},{1},{2},{3}{4}", item.Type, item.Rarity, item.Min, item.Max, Environment.NewLine);
                    }
                }

                File.WriteAllText(saveDialog.FileName, output);
            }
        }

        private void btnGenerateLoot_Click(object sender, RoutedEventArgs e)
        {
            OutputWindow outputWindow = new OutputWindow();
            outputWindow.ShowDialog();
        }

        private void dataEncounterContainers_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (programStorage.Settings.OverrideEncounterListMouseWheelScrolling)
            {
                ((DataGrid)sender).CaptureMouse();
            }
        }

        private void dataEncounterContainers_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (programStorage.Settings.OverrideEncounterListMouseWheelScrolling)
            {
                ((DataGrid)sender).ReleaseMouseCapture();
            }
        }

        //================================================================================
        // Magic Item List
        //================================================================================

        private void PopulateMagicItemTree()
        {
            List<MagicItemTreeType> treeTypes = new List<MagicItemTreeType>();

            foreach (string magicItemType in programStorage.MagicItemTypes)
            {
                MagicItemTreeType type = new MagicItemTreeType { Name = magicItemType };

                foreach (string magicItemRarity in programStorage.MagicItemRarities)
                {
                    MagicItemTreeRarity rarity = new MagicItemTreeRarity { Name = magicItemRarity };

                    foreach (MagicItem magicItem in programStorage.MagicItems.OrderBy(x=> x.Name))
                    {
                        if (magicItem.Type.Equals(magicItemType, StringComparison.CurrentCultureIgnoreCase) && magicItem.Rarity.Equals(magicItemRarity, StringComparison.CurrentCultureIgnoreCase))
                        {
                            MagicItemTreeName name = new MagicItemTreeName { Name = magicItem.Name };
                            rarity.Names.Add(name);
                        }
                    }

                    if (rarity.Names.Count > 0)
                    {
                        type.RarityList.Add(rarity);
                    }
                }

                if (type.RarityList.Count > 0)
                {
                    treeTypes.Add(type);
                }
            }

            magicItemTree.DataContext = treeTypes;
        }
        
        private void btnAddToMagicItemList_Click(object sender, RoutedEventArgs e)
        {
            if (magicItemTree.SelectedItem == null || programStorage.MagicItemTypes.Contains(magicItemTree.SelectedValue.ToString()) || programStorage.MagicItemRarities.Contains(magicItemTree.SelectedValue.ToString()))
            {
                return;
            }

            var selection = magicItemTree.SelectedValue.ToString();
            foreach (var item in programStorage.MagicItems)
            {
                if (item.Name.Equals(selection, StringComparison.CurrentCultureIgnoreCase))
                {
                    programStorage.MagicItemList.Add(item);
                    dataMagicItems.Items.Refresh();
                    break;
                }
            }
        }

        private void btnEditMagicItems_Click(object sender, RoutedEventArgs e)
        {
            MagicWindow magicWindow = new MagicWindow();
            magicWindow.ShowDialog();

            // Check to ensure that any selected magic options for the encounter have not been removed
            for (var i = programStorage.MagicItemList.Count - 1; i >= 0; i--)
            {
                if (!programStorage.MagicItems.Contains(programStorage.MagicItemList[i]))
                {
                    programStorage.MagicItemList.Remove(programStorage.MagicItemList[i]);
                }
            }

            dataMagicItems.Items.Refresh();

            // Update Magic Item Types and Rarities
            programStorage.GenerateMagicTypesAndRaritiesLists();
            PopulateRandomMagicItemControls();
            CheckRandomMagicItemList();

            PopulateMagicItemTree();
        }

        private void magicItemTree_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (programStorage.Settings.OverrideMagicItemListMouseWheelScrolling)
            {
                ((TreeView)sender).CaptureMouse();
            }
        }

        private void magicItemTree_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (programStorage.Settings.OverrideMagicItemListMouseWheelScrolling)
            {
                ((TreeView)sender).ReleaseMouseCapture();
            }
        }

        //================================================================================
        // Encounter - Guaranteed Magic Items
        //================================================================================

        private void btnMagicItemDuplicate_Click(object sender, RoutedEventArgs e)
        {
            foreach (MagicItem selection in dataMagicItems.SelectedItems)
            {
                MagicItem item = new MagicItem();
                item.Name = selection.Name;
                item.Type = selection.Type;
                item.Rarity = selection.Rarity;
                item.Description = selection.Description;


                programStorage.MagicItemList.Add(item);
            }
            dataMagicItems.Items.Refresh();
        }

        private void btnMagicItemRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataMagicItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressMagicItemListPopups)
            {
                var proceed = WarningPopup.Show("Remove Selection?", "Are you sure you want to remove the selected magic item(s)?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var selections = dataMagicItems.SelectedItems;
            for (var i = programStorage.MagicItemList.Count - 1; i >= 0; i--)
            {
                if (selections.Contains(programStorage.MagicItemList[i]))
                {
                    programStorage.MagicItemList.RemoveAt(i);
                }
            }

            dataMagicItems.Items.Refresh();
        }

        private void btnMagicItemClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataMagicItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressMagicItemListPopups)
            {
                var proceed = WarningPopup.Show("Clear Magic Items?", "Are you sure you want to clear the magic item list?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            programStorage.MagicItemList.Clear();
            dataMagicItems.Items.Refresh();
        }

        private void dataMagicItems_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (programStorage.Settings.OverrideGuaranteedMagicItemsMouseWheelScrolling)
            {
                ((DataGrid)sender).CaptureMouse();
            }
        }

        private void dataMagicItems_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (programStorage.Settings.OverrideGuaranteedMagicItemsMouseWheelScrolling)
            {
                ((DataGrid)sender).ReleaseMouseCapture();
            }
        }

        //================================================================================
        // Encounter - Random Magic Items
        //================================================================================

        private void PopulateRandomMagicItemControls()
        {
            magicItemTypes.Clear();

            foreach (MagicItem item in programStorage.MagicItems)
            {
                if (!magicItemTypes.Contains(item.Type))
                {
                    magicItemTypes.Add(item.Type);
                }
            }

            magicItemTypes.Add("Random");

            comboMagicItemType.ItemsSource = magicItemTypes;
            comboMagicItemType.Items.Refresh();
            comboMagicItemType.SelectedIndex = 0; // Set default selection
        }

        private void DetermineRandomMagicItemRarities()
        {
            magicItemRarities.Clear();

            if (comboMagicItemType.Text.Equals("Random"))
            {
                foreach (MagicItem item in programStorage.MagicItems)
                {
                    if (!magicItemRarities.Contains(item.Rarity))
                    {
                        magicItemRarities.Add(item.Rarity);
                    }
                }
            }
            else
            {
                foreach (MagicItem item in programStorage.MagicItems)
                {
                    if (magicItemTypes.Contains(item.Type) && !magicItemRarities.Contains(item.Rarity))
                    {
                        if (!item.Type.Equals(comboMagicItemType.Text))
                        {
                            continue;
                        }

                        magicItemRarities.Add(item.Rarity);
                    }
                }
            }

            magicItemRarities.Add("Random");

            comboMagicItemRarity.ItemsSource = magicItemRarities;
            comboMagicItemRarity.Items.Refresh();
            comboMagicItemRarity.SelectedIndex = 0; // Set default selection
        }

        private void CheckRandomMagicItemList()
        {
            List<RandomMagicItem> remove = new List<RandomMagicItem>();
            foreach (RandomMagicItem randomItem in programStorage.RandomMagicItemList)
            {
                var found = false;
                foreach (MagicItem magicItem in programStorage.MagicItems)
                {
                    if (magicItem.Type.Equals(randomItem.Type) && magicItem.Rarity.Equals(randomItem.Rarity))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found && !remove.Contains(randomItem))
                {
                    remove.Add(randomItem);
                }
            }

            foreach (RandomMagicItem removeItem in remove)
            {
                programStorage.RandomMagicItemList.Remove(removeItem);
            }
        }

        private void btnRandomMagicItemAdd_Click(object sender, RoutedEventArgs e)
        {
            string type = comboMagicItemType.Text;
            string rarity = comboMagicItemRarity.Text;

            if (type.Equals(String.Empty) || rarity.Equals(String.Empty))
            {
                return; // Skip if invalid selection
            }

            foreach (RandomMagicItem item in programStorage.RandomMagicItemList)
            {
                if (item.Type.Equals(type) && item.Rarity.Equals(rarity))
                {
                    return; // Skip if the type/rarity combo already exists
                }
            }

            RandomMagicItem randomItem = new RandomMagicItem(type, rarity, 0, 1);
            programStorage.RandomMagicItemList.Add(randomItem);

            dataRandomMagicItems.Items.Refresh();
        }

        private void btnRandomMagicItemRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataRandomMagicItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressRandomMagicItemListPopups)
            {
                var proceed = WarningPopup.Show("Remove Selection?", "Are you sure you want to remove the selected random magic item(s)?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var selections = dataRandomMagicItems.SelectedItems;
            for (var i = programStorage.RandomMagicItemList.Count - 1; i >= 0; i--)
            {
                if (selections.Contains(programStorage.RandomMagicItemList[i]))
                {
                    programStorage.RandomMagicItemList.RemoveAt(i);
                }
            }

            dataRandomMagicItems.Items.Refresh();
        }

        private void btnRandomMagicItemClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataRandomMagicItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressRandomMagicItemListPopups)
            {
                var proceed = WarningPopup.Show("Clear Random Magic Items?", "Are you sure you want to clear the random magic item list?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            programStorage.RandomMagicItemList.Clear();
            dataRandomMagicItems.Items.Refresh();
        }

        private void comboRandomMagicItemType_DropDownClosed(object sender, EventArgs e)
        {
            DetermineRandomMagicItemRarities();
        }

        private void comboRandomMagicItemType_TextChanged(object sender, TextChangedEventArgs e)
        {
            DetermineRandomMagicItemRarities();
        }

        private void dataRandomMagicItems_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (programStorage.Settings.OverrideRandomMagicItemsMouseWheelScrolling)
            {
                ((DataGrid)sender).CaptureMouse();
            }
        }

        private void dataRandomMagicItems_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (programStorage.Settings.OverrideRandomMagicItemsMouseWheelScrolling)
            {
                ((DataGrid)sender).ReleaseMouseCapture();
            }
        }
    }
}