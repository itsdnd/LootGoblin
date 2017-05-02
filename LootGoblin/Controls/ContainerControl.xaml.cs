using LootGoblin.Storage;
using LootGoblin.Storage.Trees;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LootGoblin.Controls
{
    /// <summary>
    /// Interaction logic for ContainerControl.xaml
    /// </summary>
    public partial class ContainerControl : UserControl
    {
        private ProgramStorage programStorage;
        private bool currentContainerHasChanged;
        private LootContainer currentContainer;
        private string originalFileName = String.Empty;
        private Item editedItem = null;

        private List<Item> armorItems;
        private List<Item> weaponItems;
        private List<Item> gearItems;

        private const int Number_Maximum = 1000000;

        public ContainerControl()
        {
            InitializeComponent();

            // Initialize needed variables
            programStorage = ProgramStorage.GetInstance();
            currentContainerHasChanged = false;
            currentContainer = null;

            // Process settings
            SuppressContainerEditPopups.IsChecked = programStorage.Settings.SuppressContainerEditPopups;

            // Set item sources
            comboContainerType.ItemsSource = programStorage.ContainerTypes;

            // Generate list of Loot Containers
            PopulateContainerTree();

            // On load, create a new container for use (not added to container list until save button is used)
            LoadContainer(new LootContainer());
            currentContainerHasChanged = false;
        }

        //================================================================================
        // Container List
        //================================================================================

        private void PopulateContainerTree()
        {
            currentContainerHasChanged = false;
            currentContainer = null;

            List<ContainerTreeType> treeTypes = new List<ContainerTreeType>();

            foreach (string containerType in programStorage.ContainerTypes)
            {
                ContainerTreeType type = new ContainerTreeType { Name = containerType };

                foreach (LootContainer container in programStorage.LootContainers)
                {
                    if (container.Type.Equals(containerType, StringComparison.CurrentCultureIgnoreCase))
                    {
                        ContainerTreeName name = new ContainerTreeName { Name = container.Name };
                        type.NameList.Add(name);
                    }
                }

                if (type.NameList.Count > 0)
                {
                    treeTypes.Add(type);
                }
            }

            containerTree.DataContext = treeTypes;
        }

        private void containerTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (containerTree.SelectedItem == null || programStorage.ContainerTypes.Contains(containerTree.SelectedValue.ToString()))
            {
                return;
            }

            var selection = containerTree.SelectedValue.ToString();
            if (currentContainer != null && currentContainer.Name.Equals(selection, StringComparison.CurrentCultureIgnoreCase))
            {
                return; // Ignore the same selection
            }

            if (currentContainerHasChanged && !programStorage.Settings.SuppressContainerEditPopups)
            {
                var proceed = WarningPopup.Show("Change Selected Item?", "Are you sure you want to change the selected loot container?\n\nThe changes made to the current container will be lost and cannot be recovered. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            foreach (var container in programStorage.LootContainers)
            {
                if (container.Name.Equals(selection, StringComparison.CurrentCultureIgnoreCase))
                {
                    LoadContainer(container);
                    break;
                }
            }

            currentContainerHasChanged = false;
        }

        private void LoadContainer(LootContainer container)
        {
            currentContainer = container;
            originalFileName = container.Name;

            txtContainerName.Text = container.Name;
            comboContainerType.Text = container.Type;
            txtContainerDescription.Text = container.Description;

            txtCopperMin.Text = container.CopperMin.ToString();
            txtCopperMax.Text = container.CopperMax.ToString();
            txtSilverMin.Text = container.SilverMin.ToString();
            txtSilverMax.Text = container.SilverMax.ToString();
            txtElectrumMin.Text = container.ElectrumMin.ToString();
            txtElectrumMax.Text = container.ElectrumMax.ToString();
            txtGoldMin.Text = container.GoldMin.ToString();
            txtGoldMax.Text = container.GoldMax.ToString();
            txtPlatinumMin.Text = container.PlatinumMin.ToString();
            txtPlatinumMax.Text = container.PlatinumMax.ToString();

            armorItems = new List<Item>(container.Armor);
            dataArmorItems.ItemsSource = armorItems;
            weaponItems = new List<Item>(container.Weapons);
            dataWeaponItems.ItemsSource = weaponItems;
            gearItems = new List<Item>(container.Gear);
            dataGearItems.ItemsSource = gearItems;

            UpdateTotalItemsAvailable(); // Update mins/maxes before setting values

            txtArmorMin.Text = container.ArmorMin.ToString();
            txtWeaponMax.Text = container.WeaponsMax.ToString();
            txtArmorMax.Text = container.ArmorMax.ToString();
            txtWeaponMax.Text = container.WeaponsMax.ToString();
            txtGearMin.Text = container.GearMin.ToString();
            txtGearMax.Text = container.GearMax.ToString();

            txtMundaneMin.Text = container.MundaneMin.ToString();
            txtMundaneMax.Text = container.MundaneMax.ToString();
        }

        //================================================================================
        // Container Details
        //================================================================================

        private void txtContainerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var name = txtContainerName.Text;
            if (currentContainer != null && name.Equals(currentContainer.Name, StringComparison.CurrentCultureIgnoreCase))
            {
                return; // Ignore if the text matches the current container's name
            }

            ChangeHappened();

            if (name.Equals(String.Empty))
            {
                txtContainerNameEmptyError.Visibility = Visibility.Visible;
                btnSave.IsEnabled = false;
                return;
            }

            foreach (LootContainer container in programStorage.LootContainers)
            {
                if (container.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    txtContainerNameError.Visibility = Visibility.Visible;
                    btnSave.IsEnabled = false;
                    return;
                }
            }

            txtContainerNameError.Visibility = Visibility.Hidden;
            txtContainerNameEmptyError.Visibility = Visibility.Hidden;
            btnSave.IsEnabled = true;
        }

        private void comboContainerType_DropDownClosed(object sender, EventArgs e)
        {
            if(currentContainer == null)
            {
                return;
            }

            if (!comboContainerType.Text.Equals(currentContainer.Type, StringComparison.CurrentCultureIgnoreCase))
            {
                ChangeHappened();
            }
        }

        private void comboContainerType_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (!comboContainerType.Text.Equals(currentContainer.Type, StringComparison.CurrentCultureIgnoreCase))
            {
                ChangeHappened();
            }
        }

        private void txtContainerDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (!comboContainerType.Text.Equals(currentContainer.Type, StringComparison.CurrentCultureIgnoreCase))
            {
                ChangeHappened();
            }
        }

        //================================================================================
        // Currency
        //================================================================================

        private void txtCopperMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtCopperMax != null && (Convert.ToInt32(txtCopperMin.Text) > Convert.ToInt32(txtCopperMax.Text)))
            {
                txtCopperMax.Text = txtCopperMin.Text;
            }

            ChangeHappened();
        }

        private void txtCopperMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (Convert.ToInt32(txtCopperMax.Text) < Convert.ToInt32(txtCopperMin.Text))
            {
                txtCopperMin.Text = txtCopperMax.Text;
            }

            ChangeHappened();
        }

        private void btnCopperMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtCopperMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtCopperMax.Text))
            {
                txtCopperMax.Text = value.ToString();
            }

            txtCopperMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnCopperMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtCopperMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtCopperMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnCopperMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtCopperMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtCopperMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnCopperMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtCopperMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtCopperMin.Text))
            {
                txtCopperMin.Text = value.ToString();
            }

            txtCopperMax.Text = value.ToString();

            ChangeHappened();
        }

        private void txtSilverMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtSilverMax != null && (Convert.ToInt32(txtSilverMin.Text) > Convert.ToInt32(txtSilverMax.Text)))
            {
                txtSilverMax.Text = txtSilverMin.Text;
            }

            ChangeHappened();
        }

        private void txtSilverMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (Convert.ToInt32(txtSilverMax.Text) < Convert.ToInt32(txtSilverMin.Text))
            {
                txtSilverMin.Text = txtSilverMax.Text;
            }

            ChangeHappened();
        }

        private void btnSilverMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtSilverMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtSilverMax.Text))
            {
                txtSilverMax.Text = value.ToString();
            }

            txtSilverMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnSilverMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtSilverMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtSilverMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnSilverMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtSilverMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtSilverMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnSilverMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtSilverMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtSilverMin.Text))
            {
                txtSilverMin.Text = value.ToString();
            }

            txtSilverMax.Text = value.ToString();

            ChangeHappened();
        }

        private void txtElectrumMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtElectrumMax != null && (Convert.ToInt32(txtElectrumMin.Text) > Convert.ToInt32(txtElectrumMax.Text)))
            {
                txtElectrumMax.Text = txtElectrumMin.Text;
            }

            ChangeHappened();
        }

        private void txtElectrumMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (Convert.ToInt32(txtElectrumMax.Text) < Convert.ToInt32(txtElectrumMin.Text))
            {
                txtElectrumMin.Text = txtElectrumMax.Text;
            }

            ChangeHappened();
        }

        private void btnElectrumMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtElectrumMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtElectrumMax.Text))
            {
                txtElectrumMax.Text = value.ToString();
            }

            txtElectrumMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnElectrumMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtElectrumMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtElectrumMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnElectrumMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtElectrumMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtElectrumMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnElectrumMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtElectrumMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtElectrumMin.Text))
            {
                txtElectrumMin.Text = value.ToString();
            }

            txtElectrumMax.Text = value.ToString();

            ChangeHappened();
        }

        private void txtGoldMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtGoldMax != null && (Convert.ToInt32(txtGoldMin.Text) > Convert.ToInt32(txtGoldMax.Text)))
            {
                txtGoldMax.Text = txtGoldMin.Text;
            }

            ChangeHappened();
        }

        private void txtGoldMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (Convert.ToInt32(txtGoldMax.Text) < Convert.ToInt32(txtGoldMin.Text))
            {
                txtGoldMin.Text = txtGoldMax.Text;
            }

            ChangeHappened();
        }

        private void btnGoldMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtGoldMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtGoldMax.Text))
            {
                txtGoldMax.Text = value.ToString();
            }

            txtGoldMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnGoldMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtGoldMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtGoldMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnGoldMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtGoldMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtGoldMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnGoldMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtGoldMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtGoldMin.Text))
            {
                txtGoldMin.Text = value.ToString();
            }

            txtGoldMax.Text = value.ToString();

            ChangeHappened();
        }

        private void txtPlatinumMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtPlatinumMax != null && (Convert.ToInt32(txtPlatinumMin.Text) > Convert.ToInt32(txtPlatinumMax.Text)))
            {
                txtPlatinumMax.Text = txtPlatinumMin.Text;
            }

            ChangeHappened();
        }

        private void txtPlatinumMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (Convert.ToInt32(txtPlatinumMax.Text) < Convert.ToInt32(txtPlatinumMin.Text))
            {
                txtPlatinumMin.Text = txtPlatinumMax.Text;
            }

            ChangeHappened();
        }

        private void btnPlatinumMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtPlatinumMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtPlatinumMax.Text))
            {
                txtPlatinumMax.Text = value.ToString();
            }

            txtPlatinumMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnPlatinumMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtPlatinumMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtPlatinumMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnPlatinumMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtPlatinumMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtPlatinumMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnPlatinumMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtPlatinumMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtPlatinumMin.Text))
            {
                txtPlatinumMin.Text = value.ToString();
            }

            txtPlatinumMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Armor Details
        //================================================================================

        private void txtArmorName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeHappened();
        }

        private void btnArmorAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnArmorAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtArmorName.Text.Equals(String.Empty)) ? "No name provided" : txtArmorName.Text;
                item.Name = name;

                var desc = (txtArmorDescription.Text.Equals(String.Empty)) ? "No description provided" : txtArmorDescription.Text;
                item.Description = desc;

                armorItems.Add(item);
                dataArmorItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtArmorName.Text.Equals(String.Empty)) ? "No name provided" : txtArmorName.Text;
                    editedItem.Name = name;

                    var desc = (txtArmorDescription.Text.Equals(String.Empty)) ? "No description provided" : txtArmorDescription.Text;
                    editedItem.Description = desc;

                    dataArmorItems.Items.Refresh();

                    // Reset inputs
                    txtArmorName.Text = String.Empty;
                    txtArmorDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnArmorAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnArmorEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataArmorItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressContainerEditPopups)
            {
                var proceed = WarningPopup.Show("Edit Item?", "Editing the selected item will clear the existing input data.\n\nThe input cannot be recovered. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            foreach (Item item in dataArmorItems.SelectedItems)
            {
                foreach (Item listItem in armorItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtArmorName.Text = item.Name;
                        txtArmorDescription.Text = item.Description;

                        btnArmorAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnArmorDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataArmorItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Description = item.Description;

                armorItems.Add(newItem);
            }

            dataArmorItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnArmorRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataArmorItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressContainerEditPopups)
            {
                var proceed = WarningPopup.Show("Remove Selection?", "Are you sure you want to remove the selected item(s)?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // Loop through each selection and remove them from the magic item list
            foreach (Item item in dataArmorItems.SelectedItems)
            {
                foreach (Item listItem in armorItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        armorItems.Remove(item);
                        break;
                    }
                }
            }

            dataArmorItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnArmorClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataArmorItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressContainerEditPopups)
            {
                var proceed = WarningPopup.Show("Clear Items?", "Are you sure you want to clear the item list?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // Remove all from the encounter list
            armorItems.Clear();
            dataArmorItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnArmorAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtArmorMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtArmorMax != null)
            {
                var min = Convert.ToInt32(txtArmorMin.Text);
                var max = Convert.ToInt32(txtArmorMax.Text);

                if (min > max)
                {
                    txtArmorMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtArmorMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtArmorMin.Text);
            var max = Convert.ToInt32(txtArmorMax.Text);
          
            if (max < min)
            {
                txtArmorMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnArmorMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtArmorMax.Text))
            {
                txtArmorMax.Text = value.ToString();
            }

            txtArmorMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArmorMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtArmorMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArmorMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtArmorMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArmorMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtArmorMin.Text))
            {
                txtArmorMin.Text = value.ToString();
            }

            txtArmorMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Weapon Details
        //================================================================================

        private void txtWeaponName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeHappened();
        }

        private void btnWeaponAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnWeaponAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtWeaponName.Text.Equals(String.Empty)) ? "No name provided" : txtWeaponName.Text;
                item.Name = name;

                var desc = (txtWeaponDescription.Text.Equals(String.Empty)) ? "No description provided" : txtWeaponDescription.Text;
                item.Description = desc;

                weaponItems.Add(item);
                dataWeaponItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtWeaponName.Text.Equals(String.Empty)) ? "No name provided" : txtWeaponName.Text;
                    editedItem.Name = name;

                    var desc = (txtWeaponDescription.Text.Equals(String.Empty)) ? "No description provided" : txtWeaponDescription.Text;
                    editedItem.Description = desc;

                    dataWeaponItems.Items.Refresh();

                    // Reset inputs
                    txtWeaponName.Text = String.Empty;
                    txtWeaponDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnWeaponAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnWeaponEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataWeaponItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressContainerEditPopups)
            {
                var proceed = WarningPopup.Show("Edit Item?", "Editing the selected item will clear the existing input data.\n\nThe input cannot be recovered. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            foreach (Item item in dataWeaponItems.SelectedItems)
            {
                foreach (Item listItem in weaponItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtWeaponName.Text = item.Name;
                        txtWeaponDescription.Text = item.Description;

                        btnWeaponAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnWeaponDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataWeaponItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Description = item.Description;

                weaponItems.Add(newItem);
            }

            dataWeaponItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnWeaponRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataWeaponItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressContainerEditPopups)
            {
                var proceed = WarningPopup.Show("Remove Selection?", "Are you sure you want to remove the selected item(s)?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // Loop through each selection and remove them from the magic item list
            foreach (Item item in dataWeaponItems.SelectedItems)
            {
                foreach (Item listItem in weaponItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        weaponItems.Remove(item);
                        break;
                    }
                }
            }

            dataWeaponItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnWeaponClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataWeaponItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressContainerEditPopups)
            {
                var proceed = WarningPopup.Show("Clear Items?", "Are you sure you want to clear the item list?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // Remove all from the encounter list
            weaponItems.Clear();
            dataWeaponItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnWeaponAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtWeaponMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtWeaponMax != null)
            {
                var min = Convert.ToInt32(txtWeaponMin.Text);
                var max = Convert.ToInt32(txtWeaponMax.Text);

                if (min > max)
                {
                    txtWeaponMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtWeaponMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtWeaponMin.Text);
            var max = Convert.ToInt32(txtWeaponMax.Text);

            if (max < min)
            {
                txtWeaponMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnWeaponMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtWeaponMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtWeaponMax.Text))
            {
                txtWeaponMax.Text = value.ToString();
            }

            txtWeaponMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnWeaponMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtWeaponMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtWeaponMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnWeaponMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtWeaponMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtWeaponMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnWeaponMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtWeaponMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtWeaponMin.Text))
            {
                txtWeaponMin.Text = value.ToString();
            }

            txtWeaponMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Gear Details
        //================================================================================

        private void txtGearName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeHappened();
        }

        private void btnGearAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnGearAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtGearName.Text.Equals(String.Empty)) ? "No name provided" : txtGearName.Text;
                item.Name = name;

                var desc = (txtGearDescription.Text.Equals(String.Empty)) ? "No description provided" : txtGearDescription.Text;
                item.Description = desc;

                gearItems.Add(item);
                dataGearItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtGearName.Text.Equals(String.Empty)) ? "No name provided" : txtGearName.Text;
                    editedItem.Name = name;

                    var desc = (txtGearDescription.Text.Equals(String.Empty)) ? "No description provided" : txtGearDescription.Text;
                    editedItem.Description = desc;

                    dataGearItems.Items.Refresh();

                    // Reset inputs
                    txtGearName.Text = String.Empty;
                    txtGearDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnGearAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnGearEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataGearItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressContainerEditPopups)
            {
                var proceed = WarningPopup.Show("Edit Item?", "Editing the selected item will clear the existing input data.\n\nThe input cannot be recovered. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            foreach (Item item in dataGearItems.SelectedItems)
            {
                foreach (Item listItem in gearItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtGearName.Text = item.Name;
                        txtGearDescription.Text = item.Description;

                        btnGearAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnGearDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataGearItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Description = item.Description;

                gearItems.Add(newItem);
            }

            dataGearItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnGearRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataGearItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressContainerEditPopups)
            {
                var proceed = WarningPopup.Show("Remove Selection?", "Are you sure you want to remove the selected item(s)?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // Loop through each selection and remove them from the magic item list
            foreach (Item item in dataGearItems.SelectedItems)
            {
                foreach (Item listItem in gearItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        gearItems.Remove(item);
                        break;
                    }
                }
            }

            dataGearItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnGearClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataGearItems.Items.Count < 1)
            {
                return;
            }

            if (!programStorage.Settings.SuppressContainerEditPopups)
            {
                var proceed = WarningPopup.Show("Clear Items?", "Are you sure you want to clear the item list?\n\nThe changes cannot be reverted. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // Remove all from the encounter list
            gearItems.Clear();
            dataGearItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnGearAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtGearMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtGearMax != null)
            {
                var min = Convert.ToInt32(txtGearMin.Text);
                var max = Convert.ToInt32(txtGearMax.Text);

                if (min > max)
                {
                    txtGearMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtGearMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtGearMin.Text);
            var max = Convert.ToInt32(txtGearMax.Text);

            if (max < min)
            {
                txtGearMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnGearMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtGearMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtGearMax.Text))
            {
                txtGearMax.Text = value.ToString();
            }

            txtGearMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnGearMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtGearMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtGearMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnGearMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtGearMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtGearMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnGearMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtGearMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtGearMin.Text))
            {
                txtGearMin.Text = value.ToString();
            }

            txtGearMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Mundane Items
        //================================================================================

        private void btnMundaneEdit_Click(object sender, RoutedEventArgs e)
        {
            MundaneItemsWindow mundane = new MundaneItemsWindow();
            mundane.ShowDialog();

            UpdateTotalItemsAvailable();
        }

        private void txtMundaneMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtMundaneMax != null)
            {
                var min = Convert.ToInt32(txtMundaneMin.Text);
                var max = Convert.ToInt32(txtMundaneMax.Text);

                if (min > max)
                {
                    txtMundaneMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtMundaneMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtMundaneMin.Text);
            var max = Convert.ToInt32(txtMundaneMax.Text);

            if (max < min)
            {
                txtMundaneMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnMundaneMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtMundaneMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtMundaneMax.Text))
            {
                txtMundaneMax.Text = value.ToString();
            }

            txtMundaneMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnMundaneMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtMundaneMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtMundaneMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnMundaneMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtMundaneMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtMundaneMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnMundaneMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtMundaneMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtMundaneMin.Text))
            {
                txtMundaneMin.Text = value.ToString();
            }

            txtMundaneMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Options
        //================================================================================

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainerHasChanged && !programStorage.Settings.SuppressMagicItemEditPopups)
            {
                var proceed = WarningPopup.Show("New Loot Container?", "Are you sure you want to create a new loot container?\n\nThe changes made to the current container will be lost and cannot be recovered. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            ClearControlInputs();
            currentContainerHasChanged = false;
        }

        private void btnDuplicate_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (currentContainerHasChanged && !programStorage.Settings.SuppressMagicItemEditPopups)
            {
                var proceed = WarningPopup.Show("Duplicate Selected Container?", "Are you sure you want to duplicate the selected container?\n\nThe changes made to the current container will be lost and cannot be recovered. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            LootContainer newContainer = new LootContainer();
            CopyContainer(currentContainer, newContainer);

            var name = String.Format("{0} - Copy", currentContainer.Name);
            currentContainer.Name = name;

            currentContainer = newContainer;
            currentContainerHasChanged = false;

            txtContainerName.Text = name;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            var proceed = WarningPopup.Show("Delete Container?", "Are you sure you want to delete the loot container?\n\nThe container cannot be recovered. Proceed?");
            if (proceed == MessageBoxResult.Yes)
            {
                string startFile = Path.Combine(Environment.CurrentDirectory, @"containers\", String.Format("{0}.json", currentContainer.Name));
                string endPath = Path.Combine(Environment.CurrentDirectory, @"containers\old\");

                Directory.CreateDirectory(endPath);

                string endFile = Path.Combine(endPath, String.Format("{0}.json", currentContainer.Name));
                File.Move(startFile, endFile);

                programStorage.LootContainers.Remove(currentContainer);

                ClearControlInputs();

                programStorage.GenerateLootContainerTypesLists();
                PopulateContainerTree();

                currentContainerHasChanged = false;
            }
        }

        private void SuppressContainerEditPopups_Checked(object sender, RoutedEventArgs e)
        {
            var suppress = SuppressContainerEditPopups.IsChecked.Value;
            programStorage.Settings.SuppressMagicItemEditPopups = suppress;
            programStorage.Settings.Save();
        }

        //================================================================================
        // Misc. / Utility
        //================================================================================

        private void ChangeHappened()
        {
            if (!currentContainerHasChanged)
            {
                currentContainerHasChanged = true;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) 
        {
            if (currentContainer == null)
            {
                return;
            }

            SaveObject();
            SaveFile();

            WarningPopup.Show("Container Save Successfully", String.Format("The loot container '{0}' has been saved successfully.", currentContainer.Name));

            ClearControlInputs();

            programStorage.GenerateLootContainerTypesLists(); // Recheck all types
            PopulateContainerTree(); // Repopulate tree
        }

        private void SaveObject()
        {
            currentContainer.Name = txtContainerName.Text;
            currentContainer.Type = comboContainerType.Text;
            currentContainer.Description = txtContainerDescription.Text;

            currentContainer.CopperMin = Convert.ToInt32(txtCopperMin.Text);
            currentContainer.CopperMax = Convert.ToInt32(txtCopperMax.Text);
            currentContainer.SilverMin = Convert.ToInt32(txtSilverMin.Text);
            currentContainer.SilverMax = Convert.ToInt32(txtSilverMax.Text);
            currentContainer.ElectrumMin = Convert.ToInt32(txtElectrumMin.Text);
            currentContainer.ElectrumMax = Convert.ToInt32(txtElectrumMax.Text);
            currentContainer.GoldMin = Convert.ToInt32(txtGoldMin.Text);
            currentContainer.GoldMax = Convert.ToInt32(txtGoldMax.Text);
            currentContainer.PlatinumMin = Convert.ToInt32(txtPlatinumMin.Text);
            currentContainer.PlatinumMax = Convert.ToInt32(txtPlatinumMax.Text);

            currentContainer.Armor = armorItems;
            currentContainer.ArmorMin = Convert.ToInt32(txtArmorMin.Text);
            currentContainer.ArmorMax = Convert.ToInt32(txtArmorMax.Text);

            currentContainer.Weapons = weaponItems;
            currentContainer.WeaponsMin = Convert.ToInt32(txtWeaponMin.Text);
            currentContainer.WeaponsMax = Convert.ToInt32(txtWeaponMax.Text);

            currentContainer.Gear = gearItems;
            currentContainer.GearMin = Convert.ToInt32(txtGearMin.Text);
            currentContainer.GearMax = Convert.ToInt32(txtGearMax.Text);

            currentContainer.MundaneMin = Convert.ToInt32(txtMundaneMin.Text);
            currentContainer.MundaneMax = Convert.ToInt32(txtMundaneMax.Text);

            if (!programStorage.LootContainers.Contains(currentContainer))
            {
                programStorage.LootContainers.Add(currentContainer);
            }
        }

        private void SaveFile()
        {
            // Get/Set Path
            string path = Path.Combine(Environment.CurrentDirectory, @"containers\");

            if (!originalFileName.Equals(String.Empty))
            { // Used to delete the old file if the loot container is renamed
                File.Delete(Path.Combine(path, String.Format("{0}.json", originalFileName)));
            }

            string fileName = String.Format("{0}.json", currentContainer.Name);

            // Create output string
            string output = JsonConvert.SerializeObject(currentContainer, Formatting.Indented);

            // Write to file
            File.WriteAllText(Path.Combine(path, fileName), output);

            originalFileName = currentContainer.Name;
        }

        private void UpdateTotalItemsAvailable()
        {
            var armor = dataArmorItems.Items.Count;
            var weapons = dataWeaponItems.Items.Count;
            var gear = dataGearItems.Items.Count;
            var mundane = programStorage.MundaneItems.Count;

            lblArmorItems.Content = armor;
            lblWeaponItems.Content = weapons;
            lblGearItems.Content = gear;
            lblTotalItems.Content = armor + weapons + gear;
            lblMundaneItems.Content = mundane;
        }

        private void CopyContainer(LootContainer currentContainer, LootContainer newContainer)
        {
            newContainer.Name = currentContainer.Name;
            newContainer.Type = currentContainer.Type;
            newContainer.Description = currentContainer.Description;

            newContainer.CopperMin = currentContainer.CopperMin;
            newContainer.CopperMax = currentContainer.CopperMax;
            newContainer.SilverMin = currentContainer.SilverMin;
            newContainer.SilverMax = currentContainer.SilverMax;
            newContainer.ElectrumMin = currentContainer.ElectrumMin;
            newContainer.ElectrumMax = currentContainer.ElectrumMax;
            newContainer.GoldMin = currentContainer.GoldMin;
            newContainer.GoldMax = currentContainer.GoldMax;
            newContainer.PlatinumMin = currentContainer.PlatinumMin;
            newContainer.PlatinumMax = currentContainer.PlatinumMax;

            newContainer.ArmorMin = currentContainer.ArmorMin;
            newContainer.ArmorMax = currentContainer.ArmorMax;
            newContainer.Armor = currentContainer.Armor;

            newContainer.WeaponsMin = currentContainer.WeaponsMin;
            newContainer.WeaponsMax = currentContainer.WeaponsMax;
            newContainer.Weapons = currentContainer.Weapons;

            newContainer.GearMin = currentContainer.GearMin;
            newContainer.GearMax = currentContainer.GearMax;
            newContainer.Gear = currentContainer.Gear;

            newContainer.MundaneMin = currentContainer.MundaneMin;
            newContainer.MundaneMax = currentContainer.MundaneMax;
        }

        private void ClearControlInputs()
        {
            currentContainer = new LootContainer();
            originalFileName = String.Empty;

            txtContainerName.Text = currentContainer.Name;
            comboContainerType.Text = currentContainer.Type;
            txtContainerDescription.Text = currentContainer.Description;

            txtCopperMin.Text = "0";
            txtCopperMax.Text = "0";
            txtSilverMin.Text = "0";
            txtSilverMax.Text = "0";
            txtElectrumMin.Text = "0";
            txtElectrumMax.Text = "0";
            txtGoldMin.Text = "0";
            txtGoldMax.Text = "0";
            txtPlatinumMin.Text = "0";
            txtPlatinumMax.Text = "0";

            txtArmorMin.Text = "0";
            txtArmorMax.Text = "0";
            armorItems = new List<Item>();
            dataArmorItems.ItemsSource = armorItems;
            dataArmorItems.Items.Refresh();

            txtWeaponMin.Text = "0";
            txtWeaponMax.Text = "0";
            weaponItems = new List<Item>();
            dataWeaponItems.ItemsSource = weaponItems;
            dataWeaponItems.Items.Refresh();

            txtGearMin.Text = "0";
            txtGearMax.Text = "0";
            gearItems = new List<Item>();
            dataGearItems.ItemsSource = gearItems;
            dataGearItems.Items.Refresh();

            txtMundaneMin.Text = "0";
            txtPlatinumMax.Text = "0";

            UpdateTotalItemsAvailable();
            currentContainerHasChanged = false;
        }

        private void txtBoxNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;

            if (txtBox.Text.Length < 0)
            {
                txtBox.Text = "0";
                return;
            }

            var value = int.Parse(txtBox.Text);
            if (value > Number_Maximum)
            {
                txtBox.Text = Number_Maximum.ToString();
            }

            if (value < 1)
            {
                txtBox.Text = "0";
            }
        }

        private void txtBoxNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            Regex regex = new Regex(@"^[0-9]+$");

            var value = 0;
            var handled = (int.TryParse(e.Text, out value) && regex.IsMatch(e.Text)) ? false : true;

            if (txtBox.Text.Length >= 7 && txtBox.SelectionLength < 1)
            {
                handled = true;
            }

            e.Handled = handled;
        }

        private void txtBoxNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }
    }
}
