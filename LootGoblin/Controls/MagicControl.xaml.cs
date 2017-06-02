using LootGoblin.Storage;
using LootGoblin.Storage.Trees;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LootGoblin.Controls
{
    /// <summary>
    /// Interaction logic for MagicControl.xaml
    /// </summary>
    public partial class MagicControl : UserControl
    {
        private ProgramStorage programStorage;
        private bool currentItemHasChanged;
        private MagicItem currentItem;

        public MagicControl()
        {
            InitializeComponent();

            // Initialize needed variables
            programStorage = ProgramStorage.GetInstance();
            currentItemHasChanged = false;
            currentItem = null;

            // Set item sources
            comboItemType.ItemsSource = programStorage.MagicItemTypes;
            comboItemRarity.ItemsSource = programStorage.MagicItemRarities;
            
            // Generate list of Magic Items
            PopulateMagicItemTree();

            // On load, create a new magic item for use (not added to magic item list until save button is used)
            LoadMagicItem(new MagicItem());
            currentItemHasChanged = false;
        }

        //================================================================================
        // Magic Item List
        //================================================================================

        private void PopulateMagicItemTree()
        {
            currentItemHasChanged = false;
            currentItem = null;

            List<MagicItemTreeType> treeTypes = new List<MagicItemTreeType>();

            foreach (string magicItemType in programStorage.MagicItemTypes)
            {
                MagicItemTreeType type = new MagicItemTreeType { Name = magicItemType };

                foreach (string magicItemRarity in programStorage.MagicItemRarities)
                {
                    MagicItemTreeRarity rarity = new MagicItemTreeRarity { Name = magicItemRarity };

                    foreach (MagicItem magicItem in programStorage.MagicItems.OrderBy(x => x.Name))
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

        private void magicItemTree_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (magicItemTree.SelectedItem == null || programStorage.MagicItemTypes.Contains(magicItemTree.SelectedValue.ToString()) || programStorage.MagicItemRarities.Contains(magicItemTree.SelectedValue.ToString()))
            {
                return;
            }

            var selection = magicItemTree.SelectedValue.ToString();
            if (currentItem != null && currentItem.Name.Equals(selection, StringComparison.CurrentCultureIgnoreCase))
            {
                return; // Ignore the same selection
            }

            if (currentItemHasChanged && !programStorage.Settings.SuppressMagicItemEditPopups)
            {
                var proceed = WarningPopup.Show("Change Selected Item?", "Are you sure you want to change the selected magic item?\n\nThe changes made to the current magic item will be lost and cannot be recovered. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            foreach (var item in programStorage.MagicItems)
            {
                if (item.Name.Equals(selection, StringComparison.CurrentCultureIgnoreCase))
                {
                    LoadMagicItem(item);
                    break;
                }
            }

            currentItemHasChanged = false;
        }

        private void LoadMagicItem(MagicItem item)
        {
            currentItem = item;
            txtItemName.Text = item.Name;
            comboItemType.Text = item.Type;
            comboItemRarity.Text = item.Rarity;
            checkAttunement.IsChecked = item.RequiresAttunement;
            txtItemDescription.Text = item.Description;
        }

        //================================================================================
        // Magic Item Management
        //================================================================================
        
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (currentItemHasChanged && !programStorage.Settings.SuppressMagicItemEditPopups)
            {
                var proceed = WarningPopup.Show("New Magic Item?", "Are you sure you want to create a new magic item?\n\nThe changes made to the current magic item will be lost and cannot be recovered. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            ClearControlInputs();
        }

        private void btnDuplicate_Click(object sender, RoutedEventArgs e)
        {
            if (currentItem == null)
            {
                return;
            }

            if (currentItemHasChanged && !programStorage.Settings.SuppressMagicItemEditPopups)
            {
                var proceed = WarningPopup.Show("Duplicate Selected Item?", "Are you sure you want to duplicate the selected magic item?\n\nThe changes made to the current magic item will be lost and cannot be recovered. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var name = String.Format("{0} - Copy", currentItem.Name);

            MagicItem newItem = new MagicItem();
            newItem.Name = name;
            newItem.Type = currentItem.Type;
            newItem.Rarity = currentItem.Rarity;
            newItem.Description = currentItem.Description;

            currentItem = newItem;
            currentItemHasChanged = false;

            txtItemName.Text = name;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (currentItem == null)
            {
                return;
            }

            var proceed = WarningPopup.Show("Delete Magic Item?", "Are you sure you want to delete the magic item?\n\nThe magic item cannot be recovered. Proceed?");
            if (proceed == MessageBoxResult.Yes)
            {
                programStorage.MagicItems.Remove(currentItem);

                SaveMagicItems();

                programStorage.GenerateMagicTypesAndRaritiesLists();
                PopulateMagicItemTree();

                ClearControlInputs();
            }
        }

        private void txtItemName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var name = txtItemName.Text;
            if (currentItem != null && name.Equals(currentItem.Name, StringComparison.CurrentCultureIgnoreCase))
            {
                return; // Ignore if the text matches the current item's name
            }

            ChangeHappened();

            if (name.Equals(String.Empty))
            {
                txtItemNameEmptyError.Visibility = Visibility.Visible;
                btnSave.IsEnabled = false;
                return;
            }

            foreach (MagicItem item in programStorage.MagicItems)
            {
                if (item.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    txtItemNameError.Visibility = Visibility.Visible;
                    btnSave.IsEnabled = false;
                    return;
                }
            }

            txtItemNameEmptyError.Visibility = Visibility.Hidden;
            txtItemNameError.Visibility = Visibility.Hidden;
            btnSave.IsEnabled = true;
        }

        private void comboItemType_DropDownClosed(object sender, EventArgs e)
        {
            if (currentItem == null)
            {
                return;
            }

            if (!comboItemType.Text.Equals(currentItem.Type, StringComparison.CurrentCultureIgnoreCase))
            {
                ChangeHappened();
            }
        }

        private void comboItemType_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentItem == null)
            {
                return;
            }

            if (!comboItemType.Text.Equals(currentItem.Type, StringComparison.CurrentCultureIgnoreCase))
            {
                ChangeHappened();
            }
        }

        private void comboItemRarity_DropDownClosed(object sender, EventArgs e)
        {
            if (currentItem == null)
            {
                return;
            }

            if (!comboItemRarity.Text.Equals(currentItem.Rarity, StringComparison.CurrentCultureIgnoreCase))
            {
                ChangeHappened();
            }
        }

        private void comboItemRarity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentItem == null)
            {
                return;
            }

            if (!comboItemRarity.Text.Equals(currentItem.Rarity, StringComparison.CurrentCultureIgnoreCase))
            {
                ChangeHappened();
            }
        }

        private void txtItemDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeHappened();
        }

        private void ChangeHappened()
        {
            if (!currentItemHasChanged)
            {
                currentItemHasChanged = true;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (currentItem == null)
            {
                return;
            }

            // Check to see if the container to save appears to be a "default" new container
            if (txtItemName.Text.Equals("New Magic Item") && comboItemType.Text.Equals("No Type") && comboItemRarity.Text.Equals("No Rarity"))
            {
                var proceed = WarningPopup.Show("Save Magic Item?", "The magic item you are attempting to save appears to be a default new magic item. Make sure you set a proper Name, Type, and Rarity. Proceed with saving?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            currentItem.Name = txtItemName.Text;
            currentItem.Type = comboItemType.Text;
            currentItem.Rarity = comboItemRarity.Text;
            currentItem.RequiresAttunement = checkAttunement.IsChecked.Value;
            currentItem.Description = txtItemDescription.Text;

            if (!programStorage.MagicItems.Contains(currentItem))
            {
                programStorage.MagicItems.Add(currentItem);
            }

            SaveMagicItems();

            programStorage.GenerateMagicTypesAndRaritiesLists(); // Recheck all types/rarities
            PopulateMagicItemTree(); // Repopulate tree

            ClearControlInputs();
        }

        private void SaveMagicItems()
        {
            // Save magic items
            string configPath = Path.Combine(Environment.CurrentDirectory, @"config\");
            Directory.CreateDirectory(configPath);

            var magicFile = Path.Combine(configPath, "magicitems.json");

            var output = JsonConvert.SerializeObject(programStorage.MagicItems, Formatting.Indented);
            File.WriteAllText(magicFile, output);
        }

        private void ClearControlInputs()
        {
            MagicItem newItem = new MagicItem();
            LoadMagicItem(newItem);
            currentItemHasChanged = false;

            comboItemType.ItemsSource = null;
            comboItemType.ItemsSource = programStorage.MagicItemTypes;
            comboItemRarity.ItemsSource = null;
            comboItemRarity.ItemsSource = programStorage.MagicItemRarities;
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
    }
}
