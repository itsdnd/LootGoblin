using LootGoblin.Storage;
using LootGoblin.Storage.Trees;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private List<Item> armorSetItems;
        private List<Item> armorPieceItems;
        private List<Item> weaponItems;
        private List<Item> ammoItems;
        private List<Item> clothingItems;
        private List<Item> clothingAccessoriesItems;
        private List<Item> foodDrinksItems;
        private List<Item> tradeGoodsItems;
        private List<Item> preciousItems;
        private List<Item> artDecorItems;
        private List<Item> booksPapersItems;
        private List<Item> otherItems;

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

                foreach (LootContainer container in programStorage.LootContainers.OrderBy(x => x.Name))
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

            armorSetItems = new List<Item>(container.ArmorSets);
            dataArmorSetsItems.ItemsSource = armorSetItems;
            armorPieceItems = new List<Item>(container.ArmorPieces);
            dataArmorPieces.ItemsSource = armorPieceItems;
            weaponItems = new List<Item>(container.Weapons);
            dataWeaponItems.ItemsSource = weaponItems;
            ammoItems = new List<Item>(container.Ammo);
            dataAmmoItems.ItemsSource = ammoItems;
            clothingItems = new List<Item>(container.Clothing);
            dataClothingItems.ItemsSource = clothingItems;
            clothingAccessoriesItems = new List<Item>(container.ClothingAccessories);
            dataClothingAccessoriesItems.ItemsSource = clothingAccessoriesItems;
            foodDrinksItems = new List<Item>(container.FoodDrinks);
            dataFoodDrinkItems.ItemsSource = foodDrinksItems;
            tradeGoodsItems = new List<Item>(container.TradeGoods);
            dataTradeGoodsItems.ItemsSource = tradeGoodsItems;
            preciousItems = new List<Item>(container.PreciousItems);
            dataPreciousItems.ItemsSource = preciousItems;
            artDecorItems = new List<Item>(container.ArtDecor);
            dataArtDecorItems.ItemsSource = artDecorItems;
            booksPapersItems = new List<Item>(container.BooksPapers);
            dataBooksPapersItems.ItemsSource = booksPapersItems;     
            otherItems = new List<Item>(container.OtherItems);
            dataOtherItems.ItemsSource = otherItems;

            UpdateTotalItemsAvailable(); // Update mins/maxes before setting values

            txtArmorSetsMin.Text = container.ArmorSetsMin.ToString();
            txtArmorSetsMax.Text = container.ArmorSetsMax.ToString();
            txtArmorPiecesMin.Text = container.ArmorPiecesMin.ToString();
            txtArmorPiecesMax.Text = container.ArmorPiecesMax.ToString();
            txtWeaponMin.Text = container.WeaponsMin.ToString();
            txtWeaponMax.Text = container.WeaponsMax.ToString();
            txtAmmoMin.Text = container.AmmoMin.ToString();
            txtAmmoMax.Text = container.AmmoMax.ToString();
            txtClothingMin.Text = container.ClothingMin.ToString();
            txtClothingMax.Text = container.ClothingMax.ToString();
            txtClothingAccessoriesMin.Text = container.ClothingAccessoriesMin.ToString();
            txtClothingAccessoriesMax.Text = container.ClothingAccessoriesMax.ToString();
            txtFoodDrinkMin.Text = container.FoodDrinksMin.ToString();
            txtFoodDrinkMax.Text = container.FoodDrinksMax.ToString();
            txtTradeGoodsMin.Text = container.TradeGoodsMin.ToString();
            txtTradeGoodsMax.Text = container.TradeGoodsMax.ToString();
            txtPreciousItemsMin.Text = container.PreciousItemsMin.ToString();
            txtPreciousItemsMax.Text = container.PreciousItemsMax.ToString();
            txtArtDecorMin.Text = container.ArtDecorMin.ToString();
            txtArtDecorMax.Text = container.ArtDecorMax.ToString();
            txtBooksPapersMin.Text = container.BooksPapersMin.ToString();
            txtBooksPapersMax.Text = container.BooksPapersMax.ToString();
            txtOtherMin.Text = container.OtherItemsMin.ToString();
            txtOtherMax.Text = container.OtherItemsMax.ToString();

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
        // Armor Sets
        //================================================================================

        private void btnArmorSetsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnArmorSetsAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtArmorSetsName.Text.Equals(String.Empty)) ? "No name provided" : txtArmorSetsName.Text;
                item.Name = name;

                item.Value = txtArmorSetsValue.Text;

                var desc = (txtArmorSetsDescription.Text.Equals(String.Empty)) ? "No description provided" : txtArmorSetsDescription.Text;
                item.Description = desc;

                armorSetItems.Add(item);
                dataArmorSetsItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtArmorSetsName.Text.Equals(String.Empty)) ? "No name provided" : txtArmorSetsName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtArmorSetsValue.Text;

                    var desc = (txtArmorSetsDescription.Text.Equals(String.Empty)) ? "No description provided" : txtArmorSetsDescription.Text;
                    editedItem.Description = desc;

                    dataArmorSetsItems.Items.Refresh();

                    // Reset inputs
                    txtArmorSetsName.Text = String.Empty;
                    txtArmorSetsValue.Text = "0gp";
                    txtArmorSetsDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnArmorSetsAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnArmorSetsEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataArmorSetsItems.Items.Count < 1)
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

            foreach (Item item in dataArmorSetsItems.SelectedItems)
            {
                foreach (Item listItem in armorSetItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtArmorSetsName.Text = item.Name;
                        txtArmorSetsValue.Text = item.Value;
                        txtArmorSetsDescription.Text = item.Description;

                        btnArmorSetsAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnArmorSetsDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataArmorSetsItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                armorSetItems.Add(newItem);
            }

            dataArmorSetsItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnArmorSetsRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataArmorSetsItems.Items.Count < 1)
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
            foreach (Item item in dataArmorSetsItems.SelectedItems)
            {
                foreach (Item listItem in armorSetItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        armorSetItems.Remove(item);
                        break;
                    }
                }
            }

            dataArmorSetsItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnArmorSetsClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataArmorSetsItems.Items.Count < 1)
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

            // Remove all from the list
            armorSetItems.Clear();
            dataArmorSetsItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnArmorSetsAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtArmorSetsMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtArmorSetsMax != null)
            {
                var min = Convert.ToInt32(txtArmorSetsMin.Text);
                var max = Convert.ToInt32(txtArmorSetsMax.Text);

                if (min > max)
                {
                    txtArmorSetsMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtArmorSetsMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtArmorSetsMin.Text);
            var max = Convert.ToInt32(txtArmorSetsMax.Text);
          
            if (max < min)
            {
                txtArmorSetsMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnArmorSetsMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorSetsMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtArmorSetsMax.Text))
            {
                txtArmorSetsMax.Text = value.ToString();
            }

            txtArmorSetsMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArmorSetsMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorSetsMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtArmorSetsMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArmorSetsMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorSetsMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtArmorSetsMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArmorSetsMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorSetsMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtArmorSetsMin.Text))
            {
                txtArmorSetsMin.Text = value.ToString();
            }

            txtArmorSetsMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Armor Pieces
        //================================================================================

        private void btnArmorPiecesAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnArmorPiecesAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtArmorPiecesName.Text.Equals(String.Empty)) ? "No name provided" : txtArmorPiecesName.Text;
                item.Name = name;

                item.Value = txtArmorPiecesValue.Text;

                var desc = (txtArmorPiecesDescription.Text.Equals(String.Empty)) ? "No description provided" : txtArmorPiecesDescription.Text;
                item.Description = desc;

                armorPieceItems.Add(item);
                dataArmorPieces.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtArmorPiecesName.Text.Equals(String.Empty)) ? "No name provided" : txtArmorPiecesName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtArmorPiecesValue.Text;

                    var desc = (txtArmorPiecesDescription.Text.Equals(String.Empty)) ? "No description provided" : txtArmorPiecesDescription.Text;
                    editedItem.Description = desc;

                    dataArmorPieces.Items.Refresh();

                    // Reset inputs
                    txtArmorPiecesName.Text = String.Empty;
                    txtArmorPiecesValue.Text = "0gp";
                    txtArmorPiecesDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnArmorPiecesAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnArmorPiecesEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataArmorPieces.Items.Count < 1)
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

            foreach (Item item in dataArmorPieces.SelectedItems)
            {
                foreach (Item listItem in armorPieceItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtArmorPiecesName.Text = item.Name;
                        txtArmorPiecesValue.Text = item.Value;
                        txtArmorPiecesDescription.Text = item.Description;

                        btnArmorPiecesAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnArmorPiecesDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataArmorPieces.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                armorPieceItems.Add(newItem);
            }

            dataArmorPieces.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnArmorPiecesRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataArmorPieces.Items.Count < 1)
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
            foreach (Item item in dataArmorPieces.SelectedItems)
            {
                foreach (Item listItem in armorPieceItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        armorPieceItems.Remove(item);
                        break;
                    }
                }
            }

            dataArmorPieces.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnArmorPiecesClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataArmorPieces.Items.Count < 1)
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

            // Remove all from the list
            armorPieceItems.Clear();
            dataArmorPieces.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnArmorPiecesAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtArmorPiecesMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtArmorPiecesMax != null)
            {
                var min = Convert.ToInt32(txtArmorPiecesMin.Text);
                var max = Convert.ToInt32(txtArmorPiecesMax.Text);

                if (min > max)
                {
                    txtArmorPiecesMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtArmorPiecesMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtArmorPiecesMin.Text);
            var max = Convert.ToInt32(txtArmorPiecesMax.Text);

            if (max < min)
            {
                txtArmorPiecesMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnArmorPiecesMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorPiecesMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtArmorPiecesMax.Text))
            {
                txtArmorPiecesMax.Text = value.ToString();
            }

            txtArmorPiecesMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArmorPiecesMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorPiecesMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtArmorPiecesMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArmorPiecesMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorPiecesMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtArmorPiecesMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArmorPiecesMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArmorPiecesMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtArmorPiecesMin.Text))
            {
                txtArmorPiecesMin.Text = value.ToString();
            }

            txtArmorPiecesMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Weapons
        //================================================================================

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

                item.Value = txtWeaponValue.Text;

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

                    editedItem.Value = txtWeaponValue.Text;

                    var desc = (txtWeaponDescription.Text.Equals(String.Empty)) ? "No description provided" : txtWeaponDescription.Text;
                    editedItem.Description = desc;

                    dataWeaponItems.Items.Refresh();

                    // Reset inputs
                    txtWeaponName.Text = String.Empty;
                    txtWeaponValue.Text = "0gp";
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
                        txtWeaponValue.Text = item.Value;
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
                newItem.Value = item.Value;
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

            // Remove all from the list
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
        // Ammo
        //================================================================================

        private void btnAmmoAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnAmmoAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtAmmoName.Text.Equals(String.Empty)) ? "No name provided" : txtAmmoName.Text;
                item.Name = name;

                item.Value = txtAmmoValue.Text;

                var desc = (txtAmmoDescription.Text.Equals(String.Empty)) ? "No description provided" : txtAmmoDescription.Text;
                item.Description = desc;

                ammoItems.Add(item);
                dataAmmoItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtAmmoName.Text.Equals(String.Empty)) ? "No name provided" : txtAmmoName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtAmmoValue.Text;

                    var desc = (txtAmmoDescription.Text.Equals(String.Empty)) ? "No description provided" : txtAmmoDescription.Text;
                    editedItem.Description = desc;

                    dataAmmoItems.Items.Refresh();

                    // Reset inputs
                    txtAmmoName.Text = String.Empty;
                    txtAmmoValue.Text = "0gp";
                    txtAmmoDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnAmmoAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnAmmoEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataAmmoItems.Items.Count < 1)
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

            foreach (Item item in dataAmmoItems.SelectedItems)
            {
                foreach (Item listItem in ammoItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtAmmoName.Text = item.Name;
                        txtAmmoValue.Text = item.Value;
                        txtAmmoDescription.Text = item.Description;

                        btnAmmoAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnAmmoDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataAmmoItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                ammoItems.Add(newItem);
            }

            dataAmmoItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnAmmoRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataAmmoItems.Items.Count < 1)
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
            foreach (Item item in dataAmmoItems.SelectedItems)
            {
                foreach (Item listItem in ammoItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        ammoItems.Remove(item);
                        break;
                    }
                }
            }

            dataAmmoItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnAmmoClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataAmmoItems.Items.Count < 1)
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

            // Remove all from the list
            ammoItems.Clear();
            dataAmmoItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnAmmoAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtAmmoMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtAmmoMax != null)
            {
                var min = Convert.ToInt32(txtAmmoMin.Text);
                var max = Convert.ToInt32(txtAmmoMax.Text);

                if (min > max)
                {
                    txtAmmoMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtAmmoMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtAmmoMin.Text);
            var max = Convert.ToInt32(txtAmmoMax.Text);

            if (max < min)
            {
                txtAmmoMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnAmmoMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtAmmoMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtAmmoMax.Text))
            {
                txtAmmoMax.Text = value.ToString();
            }

            txtAmmoMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnAmmoMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtAmmoMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtAmmoMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnAmmoMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtAmmoMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtAmmoMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnAmmoMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtAmmoMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtAmmoMin.Text))
            {
                txtAmmoMin.Text = value.ToString();
            }

            txtAmmoMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Clothing
        //================================================================================

        private void btnClothingAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnClothingAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtClothingName.Text.Equals(String.Empty)) ? "No name provided" : txtClothingName.Text;
                item.Name = name;

                item.Value = txtClothingValue.Text;

                var desc = (txtClothingDescription.Text.Equals(String.Empty)) ? "No description provided" : txtClothingDescription.Text;
                item.Description = desc;

                clothingItems.Add(item);
                dataClothingItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtClothingName.Text.Equals(String.Empty)) ? "No name provided" : txtClothingName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtClothingValue.Text;

                    var desc = (txtClothingDescription.Text.Equals(String.Empty)) ? "No description provided" : txtClothingDescription.Text;
                    editedItem.Description = desc;

                    dataClothingItems.Items.Refresh();

                    // Reset inputs
                    txtClothingName.Text = String.Empty;
                    txtClothingValue.Text = "0gp";
                    txtClothingDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnClothingAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnClothingEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataClothingItems.Items.Count < 1)
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

            foreach (Item item in dataClothingItems.SelectedItems)
            {
                foreach (Item listItem in clothingItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtClothingName.Text = item.Name;
                        txtClothingValue.Text = item.Value;
                        txtClothingDescription.Text = item.Description;

                        btnClothingAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnClothingDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataClothingItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                clothingItems.Add(newItem);
            }

            dataClothingItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnClothingRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataClothingItems.Items.Count < 1)
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
            foreach (Item item in dataClothingItems.SelectedItems)
            {
                foreach (Item listItem in clothingItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        clothingItems.Remove(item);
                        break;
                    }
                }
            }

            dataClothingItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnClothingClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataClothingItems.Items.Count < 1)
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

            // Remove all from the list
            clothingItems.Clear();
            dataClothingItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnClothingAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtClothingMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtClothingMax != null)
            {
                var min = Convert.ToInt32(txtClothingMin.Text);
                var max = Convert.ToInt32(txtClothingMax.Text);

                if (min > max)
                {
                    txtClothingMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtClothingMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtClothingMin.Text);
            var max = Convert.ToInt32(txtClothingMax.Text);

            if (max < min)
            {
                txtClothingMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnClothingMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtClothingMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtClothingMax.Text))
            {
                txtClothingMax.Text = value.ToString();
            }

            txtClothingMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnClothingMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtClothingMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtClothingMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnClothingMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtClothingMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtClothingMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnClothingMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtClothingMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtClothingMin.Text))
            {
                txtClothingMin.Text = value.ToString();
            }

            txtClothingMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Clothing Accessories
        //================================================================================

        private void btnClothingAccessoriesAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnClothingAccessoriesAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtClothingAccessoriesName.Text.Equals(String.Empty)) ? "No name provided" : txtClothingAccessoriesName.Text;
                item.Name = name;

                item.Value = txtClothingAccessoriesValue.Text;

                var desc = (txtClothingAccessoriesDescription.Text.Equals(String.Empty)) ? "No description provided" : txtClothingAccessoriesDescription.Text;
                item.Description = desc;

                clothingAccessoriesItems.Add(item);
                dataClothingAccessoriesItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtClothingAccessoriesName.Text.Equals(String.Empty)) ? "No name provided" : txtClothingAccessoriesName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtClothingAccessoriesValue.Text;

                    var desc = (txtClothingAccessoriesDescription.Text.Equals(String.Empty)) ? "No description provided" : txtClothingAccessoriesDescription.Text;
                    editedItem.Description = desc;

                    dataClothingAccessoriesItems.Items.Refresh();

                    // Reset inputs
                    txtClothingAccessoriesName.Text = String.Empty;
                    txtClothingAccessoriesValue.Text = "0gp";
                    txtClothingAccessoriesDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnClothingAccessoriesAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnClothingAccessoriesEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataClothingAccessoriesItems.Items.Count < 1)
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

            foreach (Item item in dataClothingAccessoriesItems.SelectedItems)
            {
                foreach (Item listItem in clothingAccessoriesItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtClothingAccessoriesName.Text = item.Name;
                        txtClothingAccessoriesValue.Text = item.Value;
                        txtClothingAccessoriesDescription.Text = item.Description;

                        btnClothingAccessoriesAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnClothingAccessoriesDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataClothingAccessoriesItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                clothingAccessoriesItems.Add(newItem);
            }

            dataClothingAccessoriesItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnClothingAccessoriesRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataClothingAccessoriesItems.Items.Count < 1)
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
            foreach (Item item in dataClothingAccessoriesItems.SelectedItems)
            {
                foreach (Item listItem in clothingAccessoriesItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        clothingAccessoriesItems.Remove(item);
                        break;
                    }
                }
            }

            dataClothingAccessoriesItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnClothingAccessoriesClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataClothingAccessoriesItems.Items.Count < 1)
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

            // Remove all from the list
            clothingAccessoriesItems.Clear();
            dataClothingAccessoriesItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnClothingAccessoriesAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtClothingAccessoriesMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtClothingAccessoriesMax != null)
            {
                var min = Convert.ToInt32(txtClothingAccessoriesMin.Text);
                var max = Convert.ToInt32(txtClothingAccessoriesMax.Text);

                if (min > max)
                {
                    txtClothingAccessoriesMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtClothingAccessoriesMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtClothingAccessoriesMin.Text);
            var max = Convert.ToInt32(txtClothingAccessoriesMax.Text);

            if (max < min)
            {
                txtClothingAccessoriesMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnClothingAccessoriesMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtClothingAccessoriesMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtClothingAccessoriesMax.Text))
            {
                txtClothingAccessoriesMax.Text = value.ToString();
            }

            txtClothingAccessoriesMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnClothingAccessoriesMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtClothingAccessoriesMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtClothingAccessoriesMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnClothingAccessoriesMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtClothingAccessoriesMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtClothingAccessoriesMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnClothingAccessoriesMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtClothingAccessoriesMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtClothingAccessoriesMin.Text))
            {
                txtClothingAccessoriesMin.Text = value.ToString();
            }

            txtClothingAccessoriesMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Food Drink
        //================================================================================

        private void btnFoodDrinkAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnFoodDrinkAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtFoodDrinkName.Text.Equals(String.Empty)) ? "No name provided" : txtFoodDrinkName.Text;
                item.Name = name;

                item.Value = txtFoodDrinkValue.Text;

                var desc = (txtFoodDrinkDescription.Text.Equals(String.Empty)) ? "No description provided" : txtFoodDrinkDescription.Text;
                item.Description = desc;

                foodDrinksItems.Add(item);
                dataFoodDrinkItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtFoodDrinkName.Text.Equals(String.Empty)) ? "No name provided" : txtFoodDrinkName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtFoodDrinkValue.Text;

                    var desc = (txtFoodDrinkDescription.Text.Equals(String.Empty)) ? "No description provided" : txtFoodDrinkDescription.Text;
                    editedItem.Description = desc;

                    dataFoodDrinkItems.Items.Refresh();

                    // Reset inputs
                    txtFoodDrinkName.Text = String.Empty;
                    txtFoodDrinkValue.Text = "0gp";
                    txtFoodDrinkDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnFoodDrinkAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnFoodDrinkEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataFoodDrinkItems.Items.Count < 1)
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

            foreach (Item item in dataFoodDrinkItems.SelectedItems)
            {
                foreach (Item listItem in foodDrinksItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtFoodDrinkName.Text = item.Name;
                        txtFoodDrinkValue.Text = item.Value;
                        txtFoodDrinkDescription.Text = item.Description;

                        btnFoodDrinkAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnFoodDrinkDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataFoodDrinkItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                foodDrinksItems.Add(newItem);
            }

            dataFoodDrinkItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnFoodDrinkRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataFoodDrinkItems.Items.Count < 1)
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
            foreach (Item item in dataFoodDrinkItems.SelectedItems)
            {
                foreach (Item listItem in foodDrinksItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        foodDrinksItems.Remove(item);
                        break;
                    }
                }
            }

            dataFoodDrinkItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnFoodDrinkClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataFoodDrinkItems.Items.Count < 1)
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

            // Remove all from the list
            foodDrinksItems.Clear();
            dataFoodDrinkItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnFoodDrinkAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtFoodDrinkMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtFoodDrinkMax != null)
            {
                var min = Convert.ToInt32(txtFoodDrinkMin.Text);
                var max = Convert.ToInt32(txtFoodDrinkMax.Text);

                if (min > max)
                {
                    txtFoodDrinkMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtFoodDrinkMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtFoodDrinkMin.Text);
            var max = Convert.ToInt32(txtFoodDrinkMax.Text);

            if (max < min)
            {
                txtFoodDrinkMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnFoodDrinkMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtFoodDrinkMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtFoodDrinkMax.Text))
            {
                txtFoodDrinkMax.Text = value.ToString();
            }

            txtFoodDrinkMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnFoodDrinkMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtFoodDrinkMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtFoodDrinkMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnFoodDrinkMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtFoodDrinkMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtFoodDrinkMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnFoodDrinkMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtFoodDrinkMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtFoodDrinkMin.Text))
            {
                txtFoodDrinkMin.Text = value.ToString();
            }

            txtFoodDrinkMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Trade Goods
        //================================================================================

        private void btnTradeGoodsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnTradeGoodsAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtTradeGoodsName.Text.Equals(String.Empty)) ? "No name provided" : txtTradeGoodsName.Text;
                item.Name = name;

                item.Value = txtTradeGoodsValue.Text;

                var desc = (txtTradeGoodsDescription.Text.Equals(String.Empty)) ? "No description provided" : txtTradeGoodsDescription.Text;
                item.Description = desc;

                tradeGoodsItems.Add(item);
                dataTradeGoodsItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtTradeGoodsName.Text.Equals(String.Empty)) ? "No name provided" : txtTradeGoodsName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtTradeGoodsValue.Text;

                    var desc = (txtTradeGoodsDescription.Text.Equals(String.Empty)) ? "No description provided" : txtTradeGoodsDescription.Text;
                    editedItem.Description = desc;

                    dataTradeGoodsItems.Items.Refresh();

                    // Reset inputs
                    txtTradeGoodsName.Text = String.Empty;
                    txtTradeGoodsValue.Text = "0gp";
                    txtTradeGoodsDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnTradeGoodsAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnTradeGoodsEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataTradeGoodsItems.Items.Count < 1)
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

            foreach (Item item in dataTradeGoodsItems.SelectedItems)
            {
                foreach (Item listItem in tradeGoodsItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtTradeGoodsName.Text = item.Name;
                        txtTradeGoodsValue.Text = item.Value;
                        txtTradeGoodsDescription.Text = item.Description;

                        btnTradeGoodsAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnTradeGoodsDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataTradeGoodsItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                tradeGoodsItems.Add(newItem);
            }

            dataTradeGoodsItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnTradeGoodsRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataTradeGoodsItems.Items.Count < 1)
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
            foreach (Item item in dataTradeGoodsItems.SelectedItems)
            {
                foreach (Item listItem in tradeGoodsItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        tradeGoodsItems.Remove(item);
                        break;
                    }
                }
            }

            dataTradeGoodsItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnTradeGoodsClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataTradeGoodsItems.Items.Count < 1)
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

            // Remove all from the list
            tradeGoodsItems.Clear();
            dataTradeGoodsItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnTradeGoodsAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtTradeGoodsMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtTradeGoodsMax != null)
            {
                var min = Convert.ToInt32(txtTradeGoodsMin.Text);
                var max = Convert.ToInt32(txtTradeGoodsMax.Text);

                if (min > max)
                {
                    txtTradeGoodsMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtTradeGoodsMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtTradeGoodsMin.Text);
            var max = Convert.ToInt32(txtTradeGoodsMax.Text);

            if (max < min)
            {
                txtTradeGoodsMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnTradeGoodsMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtTradeGoodsMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtTradeGoodsMax.Text))
            {
                txtTradeGoodsMax.Text = value.ToString();
            }

            txtTradeGoodsMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnTradeGoodsMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtTradeGoodsMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtTradeGoodsMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnTradeGoodsMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtTradeGoodsMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtTradeGoodsMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnTradeGoodsMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtTradeGoodsMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtTradeGoodsMin.Text))
            {
                txtTradeGoodsMin.Text = value.ToString();
            }

            txtTradeGoodsMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Precious Items
        //================================================================================

        private void btnPreciousItemsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnPreciousItemsAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtPreciousItemsName.Text.Equals(String.Empty)) ? "No name provided" : txtPreciousItemsName.Text;
                item.Name = name;

                item.Value = txtPreciousItemsValue.Text;

                var desc = (txtPreciousItemsDescription.Text.Equals(String.Empty)) ? "No description provided" : txtPreciousItemsDescription.Text;
                item.Description = desc;

                preciousItems.Add(item);
                dataPreciousItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtPreciousItemsName.Text.Equals(String.Empty)) ? "No name provided" : txtPreciousItemsName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtPreciousItemsValue.Text;

                    var desc = (txtPreciousItemsDescription.Text.Equals(String.Empty)) ? "No description provided" : txtPreciousItemsDescription.Text;
                    editedItem.Description = desc;

                    dataPreciousItems.Items.Refresh();

                    // Reset inputs
                    txtPreciousItemsName.Text = String.Empty;
                    txtPreciousItemsValue.Text = "0gp";
                    txtPreciousItemsDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnPreciousItemsAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnPreciousItemsEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataPreciousItems.Items.Count < 1)
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

            foreach (Item item in dataPreciousItems.SelectedItems)
            {
                foreach (Item listItem in preciousItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtPreciousItemsName.Text = item.Name;
                        txtPreciousItemsValue.Text = item.Value;
                        txtPreciousItemsDescription.Text = item.Description;

                        btnPreciousItemsAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnPreciousItemsDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataPreciousItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                preciousItems.Add(newItem);
            }

            dataPreciousItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnPreciousItemsRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataPreciousItems.Items.Count < 1)
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
            foreach (Item item in dataPreciousItems.SelectedItems)
            {
                foreach (Item listItem in preciousItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        preciousItems.Remove(item);
                        break;
                    }
                }
            }

            dataPreciousItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnPreciousItemsClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataPreciousItems.Items.Count < 1)
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

            // Remove all from the list
            preciousItems.Clear();
            dataPreciousItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnPreciousItemsAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtPreciousItemsMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtPreciousItemsMax != null)
            {
                var min = Convert.ToInt32(txtPreciousItemsMin.Text);
                var max = Convert.ToInt32(txtPreciousItemsMax.Text);

                if (min > max)
                {
                    txtPreciousItemsMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtPreciousItemsMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtPreciousItemsMin.Text);
            var max = Convert.ToInt32(txtPreciousItemsMax.Text);

            if (max < min)
            {
                txtPreciousItemsMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnPreciousItemsMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtPreciousItemsMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtPreciousItemsMax.Text))
            {
                txtPreciousItemsMax.Text = value.ToString();
            }

            txtPreciousItemsMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnPreciousItemsMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtPreciousItemsMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtPreciousItemsMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnPreciousItemsMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtPreciousItemsMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtPreciousItemsMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnPreciousItemsMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtPreciousItemsMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtPreciousItemsMin.Text))
            {
                txtPreciousItemsMin.Text = value.ToString();
            }

            txtPreciousItemsMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Art Decor
        //================================================================================

        private void btnArtDecorAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnArtDecorAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtArtDecorName.Text.Equals(String.Empty)) ? "No name provided" : txtArtDecorName.Text;
                item.Name = name;

                item.Value = txtArtDecorValue.Text;

                var desc = (txtArtDecorDescription.Text.Equals(String.Empty)) ? "No description provided" : txtArtDecorDescription.Text;
                item.Description = desc;

                artDecorItems.Add(item);
                dataArtDecorItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtArtDecorName.Text.Equals(String.Empty)) ? "No name provided" : txtArtDecorName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtArtDecorValue.Text;

                    var desc = (txtArtDecorDescription.Text.Equals(String.Empty)) ? "No description provided" : txtArtDecorDescription.Text;
                    editedItem.Description = desc;

                    dataArtDecorItems.Items.Refresh();

                    // Reset inputs
                    txtArtDecorName.Text = String.Empty;
                    txtArtDecorValue.Text = "0gp";
                    txtArtDecorDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnArtDecorAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnArtDecorEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataArtDecorItems.Items.Count < 1)
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

            foreach (Item item in dataArtDecorItems.SelectedItems)
            {
                foreach (Item listItem in artDecorItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtArtDecorName.Text = item.Name;
                        txtArtDecorValue.Text = item.Value;
                        txtArtDecorDescription.Text = item.Description;

                        btnArtDecorAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnArtDecorDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataArtDecorItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                artDecorItems.Add(newItem);
            }

            dataArtDecorItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnArtDecorRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataArtDecorItems.Items.Count < 1)
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
            foreach (Item item in dataArtDecorItems.SelectedItems)
            {
                foreach (Item listItem in artDecorItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        artDecorItems.Remove(item);
                        break;
                    }
                }
            }

            dataArtDecorItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnArtDecorClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataArtDecorItems.Items.Count < 1)
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

            // Remove all from the list
            artDecorItems.Clear();
            dataArtDecorItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnArtDecorAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtArtDecorMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtArtDecorMax != null)
            {
                var min = Convert.ToInt32(txtArtDecorMin.Text);
                var max = Convert.ToInt32(txtArtDecorMax.Text);

                if (min > max)
                {
                    txtArtDecorMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtArtDecorMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtArtDecorMin.Text);
            var max = Convert.ToInt32(txtArtDecorMax.Text);

            if (max < min)
            {
                txtArtDecorMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnArtDecorMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArtDecorMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtArtDecorMax.Text))
            {
                txtArtDecorMax.Text = value.ToString();
            }

            txtArtDecorMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArtDecorMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArtDecorMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtArtDecorMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArtDecorMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArtDecorMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtArtDecorMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnArtDecorMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtArtDecorMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtArtDecorMin.Text))
            {
                txtArtDecorMin.Text = value.ToString();
            }

            txtArtDecorMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Books Papers
        //================================================================================

        private void btnBooksPapersAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnBooksPapersAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtBooksPapersName.Text.Equals(String.Empty)) ? "No name provided" : txtBooksPapersName.Text;
                item.Name = name;

                item.Value = txtBooksPapersValue.Text;

                var desc = (txtBooksPapersDescription.Text.Equals(String.Empty)) ? "No description provided" : txtBooksPapersDescription.Text;
                item.Description = desc;

                booksPapersItems.Add(item);
                dataBooksPapersItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtBooksPapersName.Text.Equals(String.Empty)) ? "No name provided" : txtBooksPapersName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtBooksPapersValue.Text;

                    var desc = (txtBooksPapersDescription.Text.Equals(String.Empty)) ? "No description provided" : txtBooksPapersDescription.Text;
                    editedItem.Description = desc;

                    dataBooksPapersItems.Items.Refresh();

                    // Reset inputs
                    txtBooksPapersName.Text = String.Empty;
                    txtBooksPapersValue.Text = "0gp";
                    txtBooksPapersDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnBooksPapersAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnBooksPapersEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataBooksPapersItems.Items.Count < 1)
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

            foreach (Item item in dataBooksPapersItems.SelectedItems)
            {
                foreach (Item listItem in booksPapersItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtBooksPapersName.Text = item.Name;
                        txtBooksPapersValue.Text = item.Value;
                        txtBooksPapersDescription.Text = item.Description;

                        btnBooksPapersAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnBooksPapersDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataBooksPapersItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                booksPapersItems.Add(newItem);
            }

            dataBooksPapersItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnBooksPapersRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataBooksPapersItems.Items.Count < 1)
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
            foreach (Item item in dataBooksPapersItems.SelectedItems)
            {
                foreach (Item listItem in booksPapersItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        booksPapersItems.Remove(item);
                        break;
                    }
                }
            }

            dataBooksPapersItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnBooksPapersClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataBooksPapersItems.Items.Count < 1)
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

            // Remove all from the list
            booksPapersItems.Clear();
            dataBooksPapersItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnBooksPapersAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtBooksPapersMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtBooksPapersMax != null)
            {
                var min = Convert.ToInt32(txtBooksPapersMin.Text);
                var max = Convert.ToInt32(txtBooksPapersMax.Text);

                if (min > max)
                {
                    txtBooksPapersMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtBooksPapersMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtBooksPapersMin.Text);
            var max = Convert.ToInt32(txtBooksPapersMax.Text);

            if (max < min)
            {
                txtBooksPapersMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnBooksPapersMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtBooksPapersMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtBooksPapersMax.Text))
            {
                txtBooksPapersMax.Text = value.ToString();
            }

            txtBooksPapersMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnBooksPapersMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtBooksPapersMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtBooksPapersMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnBooksPapersMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtBooksPapersMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtBooksPapersMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnBooksPapersMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtBooksPapersMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtBooksPapersMin.Text))
            {
                txtBooksPapersMin.Text = value.ToString();
            }

            txtBooksPapersMax.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Other Items
        //================================================================================

        private void btnOtherAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            if (btnOtherAddText.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                Item item = new Item();

                var name = (txtOtherName.Text.Equals(String.Empty)) ? "No name provided" : txtOtherName.Text;
                item.Name = name;

                item.Value = txtOtherValue.Text;

                var desc = (txtOtherDescription.Text.Equals(String.Empty)) ? "No description provided" : txtOtherDescription.Text;
                item.Description = desc;

                otherItems.Add(item);
                dataOtherItems.Items.Refresh();

                ChangeHappened();
            }
            else
            { // Update item
                if (editedItem != null)
                {
                    var name = (txtOtherName.Text.Equals(String.Empty)) ? "No name provided" : txtOtherName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtOtherValue.Text;

                    var desc = (txtOtherDescription.Text.Equals(String.Empty)) ? "No description provided" : txtOtherDescription.Text;
                    editedItem.Description = desc;

                    dataOtherItems.Items.Refresh();

                    // Reset inputs
                    txtOtherName.Text = String.Empty;
                    txtOtherValue.Text = "0gp";
                    txtOtherDescription.Text = String.Empty;

                    ChangeHappened();
                }

                btnOtherAddText.Text = "Add Item";
            }

            UpdateTotalItemsAvailable();
        }

        private void btnOtherEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataOtherItems.Items.Count < 1)
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

            foreach (Item item in dataOtherItems.SelectedItems)
            {
                foreach (Item listItem in otherItems)
                {
                    if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        editedItem = item;

                        txtOtherName.Text = item.Name;
                        txtOtherValue.Text = item.Value;
                        txtOtherDescription.Text = item.Description;

                        btnOtherAddText.Text = "Update Item";

                        return; // Get only first selected item
                    }
                }
            }
        }

        private void btnOtherDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataOtherItems.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                otherItems.Add(newItem);
            }

            dataOtherItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnOtherRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataOtherItems.Items.Count < 1)
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
            foreach (Item item in dataOtherItems.SelectedItems)
            {
                foreach (Item listItem in otherItems)
                {
                    if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        otherItems.Remove(item);
                        break;
                    }
                }
            }

            dataOtherItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            ChangeHappened();
        }

        private void btnOtherClear_Click(object sender, RoutedEventArgs e)
        {
            if (dataOtherItems.Items.Count < 1)
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

            // Remove all from the list
            otherItems.Clear();
            dataOtherItems.Items.Refresh(); // Refresh the item source

            UpdateTotalItemsAvailable();

            btnOtherAddText.Text = "Add To Item List"; // Failsafe

            ChangeHappened();
        }

        private void txtOtherMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            if (txtOtherMax != null)
            {
                var min = Convert.ToInt32(txtOtherMin.Text);
                var max = Convert.ToInt32(txtOtherMax.Text);

                if (min > max)
                {
                    txtOtherMax.Text = min.ToString();
                }
            }

            ChangeHappened();
        }

        private void txtOtherMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            var min = Convert.ToInt32(txtOtherMin.Text);
            var max = Convert.ToInt32(txtOtherMax.Text);

            if (max < min)
            {
                txtOtherMin.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnOtherMinUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtOtherMin.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            if (value > Convert.ToInt32(txtOtherMax.Text))
            {
                txtOtherMax.Text = value.ToString();
            }

            txtOtherMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnOtherMinDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtOtherMin.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            txtOtherMin.Text = value.ToString();

            ChangeHappened();
        }

        private void btnOtherMaxUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtOtherMax.Text);
            value++;

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            txtOtherMax.Text = value.ToString();

            ChangeHappened();
        }

        private void btnOtherMaxDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(txtOtherMax.Text);
            value--;

            if (value < 0)
            {
                value = 0;
            }

            if (value < Convert.ToInt32(txtOtherMin.Text))
            {
                txtOtherMin.Text = value.ToString();
            }

            txtOtherMax.Text = value.ToString();

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
            newContainer.Name = name;

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
                
                programStorage.GenerateLootContainerTypesLists();
                PopulateContainerTree();

                ClearControlInputs();
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
            
            programStorage.GenerateLootContainerTypesLists(); // Recheck all types
            PopulateContainerTree(); // Repopulate tree

            ClearControlInputs();
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

            currentContainer.MundaneMin = Convert.ToInt32(txtMundaneMin.Text);
            currentContainer.MundaneMax = Convert.ToInt32(txtMundaneMax.Text);

            currentContainer.ArmorSets = armorSetItems;
            currentContainer.ArmorSetsMin = Convert.ToInt32(txtArmorSetsMin.Text);
            currentContainer.ArmorSetsMax = Convert.ToInt32(txtArmorSetsMax.Text);
            currentContainer.ArmorPieces = armorPieceItems;
            currentContainer.ArmorPiecesMin = Convert.ToInt32(txtArmorPiecesMin.Text);
            currentContainer.ArmorPiecesMax = Convert.ToInt32(txtArmorPiecesMax.Text);
            currentContainer.Weapons = weaponItems;
            currentContainer.WeaponsMin = Convert.ToInt32(txtWeaponMin.Text);
            currentContainer.WeaponsMax = Convert.ToInt32(txtWeaponMax.Text);
            currentContainer.Ammo = ammoItems;
            currentContainer.AmmoMin = Convert.ToInt32(txtAmmoMin.Text);
            currentContainer.AmmoMax = Convert.ToInt32(txtAmmoMax.Text);
            currentContainer.Clothing = clothingItems;
            currentContainer.ClothingMin = Convert.ToInt32(txtClothingMin.Text);
            currentContainer.ClothingMax = Convert.ToInt32(txtClothingMax.Text);
            currentContainer.ClothingAccessories = clothingAccessoriesItems;
            currentContainer.ClothingAccessoriesMin = Convert.ToInt32(txtClothingAccessoriesMin.Text);
            currentContainer.ClothingAccessoriesMax = Convert.ToInt32(txtClothingAccessoriesMax.Text);
            currentContainer.FoodDrinks = foodDrinksItems;
            currentContainer.FoodDrinksMin = Convert.ToInt32(txtFoodDrinkMin.Text);
            currentContainer.FoodDrinksMax = Convert.ToInt32(txtFoodDrinkMax.Text);
            currentContainer.TradeGoods = tradeGoodsItems;
            currentContainer.TradeGoodsMin = Convert.ToInt32(txtTradeGoodsMin.Text);
            currentContainer.TradeGoodsMax = Convert.ToInt32(txtTradeGoodsMax.Text);
            currentContainer.PreciousItems = preciousItems;
            currentContainer.PreciousItemsMin = Convert.ToInt32(txtPreciousItemsMin.Text);
            currentContainer.PreciousItemsMax = Convert.ToInt32(txtPreciousItemsMax.Text);
            currentContainer.ArtDecor = artDecorItems;
            currentContainer.ArtDecorMin = Convert.ToInt32(txtArtDecorMin.Text);
            currentContainer.ArtDecorMax = Convert.ToInt32(txtArtDecorMax.Text);
            currentContainer.BooksPapers = booksPapersItems;
            currentContainer.BooksPapersMin = Convert.ToInt32(txtBooksPapersMin.Text);
            currentContainer.BooksPapersMax = Convert.ToInt32(txtBooksPapersMax.Text);
            currentContainer.OtherItems = otherItems;
            currentContainer.OtherItemsMin = Convert.ToInt32(txtOtherMin.Text);
            currentContainer.OtherItemsMax = Convert.ToInt32(txtOtherMax.Text);

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
            lblMundaneItems.Content = programStorage.MundaneItems.Count;
            lblArmorSetsItems.Content = dataArmorSetsItems.Items.Count;
            lblArmorPiecesItems.Content = dataArmorPieces.Items.Count;
            lblWeaponItems.Content = dataWeaponItems.Items.Count;
            lblAmmoItems.Content = dataAmmoItems.Items.Count;
            lblClothingItems.Content = dataClothingItems.Items.Count;
            lblClothingAccessoriesItems.Content = dataClothingAccessoriesItems.Items.Count;
            lblFoodDrinkItems.Content = dataFoodDrinkItems.Items.Count;
            lblTradeGoodsItems.Content = dataTradeGoodsItems.Items.Count;
            lblPreciousItems.Content = dataPreciousItems.Items.Count;
            lblArtDecorItems.Content = dataArtDecorItems.Items.Count;
            lblBooksPapersItems.Content = dataBooksPapersItems.Items.Count;
            lblOtherItems.Content = dataOtherItems.Items.Count;
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

            newContainer.MundaneMin = currentContainer.MundaneMin;
            newContainer.MundaneMax = currentContainer.MundaneMax;

            newContainer.ArmorSetsMin = currentContainer.ArmorSetsMin;
            newContainer.ArmorSetsMax = currentContainer.ArmorSetsMax;
            newContainer.ArmorSets = currentContainer.ArmorSets;
            newContainer.ArmorPiecesMin = currentContainer.ArmorPiecesMin;
            newContainer.ArmorPiecesMax = currentContainer.ArmorPiecesMax;
            newContainer.ArmorPieces = currentContainer.ArmorPieces;
            newContainer.WeaponsMin = currentContainer.WeaponsMin;
            newContainer.WeaponsMax = currentContainer.WeaponsMax;
            newContainer.Weapons = currentContainer.Weapons;
            newContainer.AmmoMin = currentContainer.AmmoMin;
            newContainer.AmmoMax = currentContainer.AmmoMax;
            newContainer.Ammo = currentContainer.Ammo;
            newContainer.ClothingMin = currentContainer.ClothingMin;
            newContainer.ClothingMax = currentContainer.ClothingMax;
            newContainer.Clothing = currentContainer.Clothing;
            newContainer.ClothingAccessoriesMin = currentContainer.ClothingAccessoriesMin;
            newContainer.ClothingAccessoriesMax = currentContainer.ClothingAccessoriesMax;
            newContainer.ClothingAccessories = currentContainer.ClothingAccessories;
            newContainer.FoodDrinksMin = currentContainer.FoodDrinksMin;
            newContainer.FoodDrinksMax = currentContainer.FoodDrinksMax;
            newContainer.FoodDrinks = currentContainer.FoodDrinks;
            newContainer.TradeGoodsMin = currentContainer.TradeGoodsMin;
            newContainer.TradeGoodsMax = currentContainer.TradeGoodsMax;
            newContainer.TradeGoods = currentContainer.TradeGoods;
            newContainer.PreciousItemsMin = currentContainer.PreciousItemsMin;
            newContainer.PreciousItemsMax = currentContainer.PreciousItemsMax;
            newContainer.PreciousItems = currentContainer.PreciousItems;
            newContainer.ArtDecorMin = currentContainer.ArtDecorMin;
            newContainer.ArtDecorMax = currentContainer.ArtDecorMax;
            newContainer.ArtDecor = currentContainer.ArtDecor;
            newContainer.BooksPapersMin = currentContainer.BooksPapersMin;
            newContainer.BooksPapersMax = currentContainer.BooksPapersMax;
            newContainer.BooksPapers = currentContainer.BooksPapers;
            newContainer.OtherItemsMin = currentContainer.OtherItemsMin;
            newContainer.OtherItemsMax = currentContainer.OtherItemsMax;
            newContainer.OtherItems = currentContainer.OtherItems;
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

            txtMundaneMin.Text = "0";
            txtPlatinumMax.Text = "0";

            txtArmorSetsMin.Text = "0";
            txtArmorSetsMax.Text = "0";
            armorSetItems = new List<Item>();
            dataArmorSetsItems.ItemsSource = armorSetItems;
            dataArmorSetsItems.Items.Refresh();

            txtArmorPiecesMin.Text = "0";
            txtArmorPiecesMax.Text = "0";
            armorPieceItems = new List<Item>();
            dataArmorPieces.ItemsSource = armorPieceItems;
            dataArmorPieces.Items.Refresh();

            txtWeaponMin.Text = "0";
            txtWeaponMax.Text = "0";
            weaponItems = new List<Item>();
            dataWeaponItems.ItemsSource = weaponItems;
            dataWeaponItems.Items.Refresh();

            txtAmmoMin.Text = "0";
            txtAmmoMax.Text = "0";
            ammoItems = new List<Item>();
            dataAmmoItems.ItemsSource = ammoItems;
            dataAmmoItems.Items.Refresh();

            txtClothingMin.Text = "0";
            txtClothingMax.Text = "0";
            clothingItems = new List<Item>();
            dataClothingItems.ItemsSource = clothingItems;
            dataClothingItems.Items.Refresh();

            txtClothingAccessoriesMin.Text = "0";
            txtClothingAccessoriesMax.Text = "0";
            clothingAccessoriesItems = new List<Item>();
            dataClothingAccessoriesItems.ItemsSource = clothingAccessoriesItems;
            dataClothingAccessoriesItems.Items.Refresh();

            txtFoodDrinkMin.Text = "0";
            txtFoodDrinkMax.Text = "0";
            foodDrinksItems = new List<Item>();
            dataFoodDrinkItems.ItemsSource = foodDrinksItems;
            dataFoodDrinkItems.Items.Refresh();

            txtTradeGoodsMin.Text = "0";
            txtTradeGoodsMax.Text = "0";
            tradeGoodsItems = new List<Item>();
            dataTradeGoodsItems.ItemsSource = tradeGoodsItems;
            dataTradeGoodsItems.Items.Refresh();

            txtPreciousItemsMin.Text = "0";
            txtPreciousItemsMax.Text = "0";
            preciousItems = new List<Item>();
            dataPreciousItems.ItemsSource = preciousItems;
            dataPreciousItems.Items.Refresh();

            txtArtDecorMin.Text = "0";
            txtArtDecorMax.Text = "0";
            artDecorItems = new List<Item>();
            dataArtDecorItems.ItemsSource = artDecorItems;
            dataArtDecorItems.Items.Refresh();

            txtBooksPapersMin.Text = "0";
            txtBooksPapersMax.Text = "0";
            booksPapersItems = new List<Item>();
            dataBooksPapersItems.ItemsSource = booksPapersItems;
            dataBooksPapersItems.Items.Refresh();

            txtOtherMin.Text = "0";
            txtOtherMax.Text = "0";
            otherItems = new List<Item>();
            dataOtherItems.ItemsSource = otherItems;
            dataOtherItems.Items.Refresh();

            UpdateTotalItemsAvailable();
            currentContainerHasChanged = false;

            comboContainerType.ItemsSource = null;
            comboContainerType.ItemsSource = programStorage.ContainerTypes;
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
