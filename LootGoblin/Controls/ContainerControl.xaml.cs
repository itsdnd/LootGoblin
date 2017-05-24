using LootGoblin.Storage;
using LootGoblin.Storage.Trees;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        private ObservableCollection<Item> ArmorSetsList;
        private ObservableCollection<Item> ArmorPiecesList;
        private ObservableCollection<Item> WeaponsList;
        private ObservableCollection<Item> AmmoList;
        private ObservableCollection<Item> ClothingList;
        private ObservableCollection<Item> ClothingAccessoriesList;
        private ObservableCollection<Item> FoodDrinksList;
        private ObservableCollection<Item> TradeGoodsList;
        private ObservableCollection<Item> PreciousItemsList;
        private ObservableCollection<Item> ArtDecorList;
        private ObservableCollection<Item> BooksPapersList;
        private ObservableCollection<Item> OtherItemsList;

        private List<string> containerNames;

        private const int Number_Maximum = 1000000;

        public ContainerControl()
        {
            InitializeComponent();

            // Initialize needed variables
            programStorage = ProgramStorage.GetInstance();
            currentContainerHasChanged = false;
            currentContainer = null;

            // Set item sources
            comboContainerType.ItemsSource = programStorage.ContainerTypes;

            containerNames = new List<string>();
            foreach (LootContainer container in programStorage.LootContainers)
            {
                containerNames.Add(container.Name);
            }

            comboArmorSetsImport.ItemsSource = containerNames;
            comboArmorPiecesImport.ItemsSource = containerNames;
            comboWeaponsImport.ItemsSource = containerNames;
            comboAmmoImport.ItemsSource = containerNames;
            comboClothingImport.ItemsSource = containerNames;
            comboClothingAccessoriesImport.ItemsSource = containerNames;
            comboFoodDrinksImport.ItemsSource = containerNames;
            comboTradeGoodsImport.ItemsSource = containerNames;
            comboPreciousItemsImport.ItemsSource = containerNames;
            comboArtDecorImport.ItemsSource = containerNames;
            comboBooksPapersImport.ItemsSource = containerNames;
            comboOtherItemsImport.ItemsSource = containerNames;

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

            ArmorSetsList = new ObservableCollection<Item>(container.ArmorSets);
            dataArmorSets.ItemsSource = ArmorSetsList;
            ArmorPiecesList = new ObservableCollection<Item>(container.ArmorPieces);
            dataArmorPieces.ItemsSource = ArmorPiecesList;
            WeaponsList = new ObservableCollection<Item>(container.Weapons);
            dataWeapons.ItemsSource = WeaponsList;
            AmmoList = new ObservableCollection<Item>(container.Ammo);
            dataAmmo.ItemsSource = AmmoList;
            ClothingList = new ObservableCollection<Item>(container.Clothing);
            dataClothing.ItemsSource = ClothingList;
            ClothingAccessoriesList = new ObservableCollection<Item>(container.ClothingAccessories);
            dataClothingAccessories.ItemsSource = ClothingAccessoriesList;
            FoodDrinksList = new ObservableCollection<Item>(container.FoodDrinks);
            dataFoodDrinks.ItemsSource = FoodDrinksList;
            TradeGoodsList = new ObservableCollection<Item>(container.TradeGoods);
            dataTradeGoods.ItemsSource = TradeGoodsList;
            PreciousItemsList = new ObservableCollection<Item>(container.PreciousItems);
            dataPreciousItems.ItemsSource = PreciousItemsList;
            ArtDecorList = new ObservableCollection<Item>(container.ArtDecor);
            dataArtDecor.ItemsSource = ArtDecorList;
            BooksPapersList = new ObservableCollection<Item>(container.BooksPapers);
            dataBooksPapers.ItemsSource = BooksPapersList;     
            OtherItemsList = new ObservableCollection<Item>(container.OtherItems);
            dataOtherItems.ItemsSource = OtherItemsList;

            UpdateAvailableCategoryItemAmounts();

            txtArmorSetsMin.Text = container.ArmorSetsMin.ToString();
            txtArmorSetsMax.Text = container.ArmorSetsMax.ToString();
            txtArmorPiecesMin.Text = container.ArmorPiecesMin.ToString();
            txtArmorPiecesMax.Text = container.ArmorPiecesMax.ToString();
            txtWeaponsMin.Text = container.WeaponsMin.ToString();
            txtWeaponsMax.Text = container.WeaponsMax.ToString();
            txtAmmoMin.Text = container.AmmoMin.ToString();
            txtAmmoMax.Text = container.AmmoMax.ToString();
            txtClothingMin.Text = container.ClothingMin.ToString();
            txtClothingMax.Text = container.ClothingMax.ToString();
            txtClothingAccessoriesMin.Text = container.ClothingAccessoriesMin.ToString();
            txtClothingAccessoriesMax.Text = container.ClothingAccessoriesMax.ToString();
            txtFoodDrinksMin.Text = container.FoodDrinksMin.ToString();
            txtFoodDrinksMax.Text = container.FoodDrinksMax.ToString();
            txtTradeGoodsMin.Text = container.TradeGoodsMin.ToString();
            txtTradeGoodsMax.Text = container.TradeGoodsMax.ToString();
            txtPreciousItemsMin.Text = container.PreciousItemsMin.ToString();
            txtPreciousItemsMax.Text = container.PreciousItemsMax.ToString();
            txtArtDecorMin.Text = container.ArtDecorMin.ToString();
            txtArtDecorMax.Text = container.ArtDecorMax.ToString();
            txtBooksPapersMin.Text = container.BooksPapersMin.ToString();
            txtBooksPapersMax.Text = container.BooksPapersMax.ToString();
            txtOtherItemsMin.Text = container.OtherItemsMin.ToString();
            txtOtherItemsMax.Text = container.OtherItemsMax.ToString();

            txtMundaneItemsMin.Text = container.MundaneMin.ToString();
            txtMundaneItemsMax.Text = container.MundaneMax.ToString();

            txtTrinketsMin.Text = container.TrinketMin.ToString();
            txtTrinketsMax.Text = container.TrinketMax.ToString();
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
        // Mundane Items & Trinkets
        //================================================================================

        private void btnMundaneEdit_Click(object sender, RoutedEventArgs e)
        {
            MundaneItemsWindow mundane = new MundaneItemsWindow();
            mundane.ShowDialog();

            UpdateAvailableCategoryItemAmounts();
        }


        private void btnTrinketEdit_Click(object sender, RoutedEventArgs e)
        {
            TrinketsWindow trinket = new TrinketsWindow();
            trinket.ShowDialog();

            UpdateAvailableCategoryItemAmounts();
        }

        //================================================================================
        //  Min/Max Manipulation
        //================================================================================

        private void txtMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            // Get the name of the textbox that sent the textchanged event and determine what "category" it is.
            var category = (sender as TextBox).Name.Replace("txt", "").Replace("Max", "").Replace("Min", "");

            // Get controls/fields relevant to the category
            var minTextBox = (TextBox)this.FindName(String.Format("txt{0}Min", category));
            var maxTextBox = (TextBox)this.FindName(String.Format("txt{0}Max", category));

            // Check for null/not found controls/fields
            if (minTextBox == null || maxTextBox == null)
            {
                return;
            }

            var min = Convert.ToInt32(minTextBox.Text);
            var max = Convert.ToInt32(maxTextBox.Text);

            if (min > max)
            {
                maxTextBox.Text = min.ToString();
            }

            ChangeHappened();
        }

        private void txtMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxNumber_TextChanged(sender, e);

            // Get the name of the textbox that sent the textchanged event and determine what "category" it is.
            var category = (sender as TextBox).Name.Replace("txt", "").Replace("Max", "").Replace("Min", "");

            // Get controls/fields relevant to the category
            var minTextBox = (TextBox)this.FindName(String.Format("txt{0}Min", category));
            var maxTextBox = (TextBox)this.FindName(String.Format("txt{0}Max", category));

            // Check for null/not found controls/fields
            if (minTextBox == null || maxTextBox == null)
            {
                return;
            }

            var min = Convert.ToInt32(minTextBox.Text);
            var max = Convert.ToInt32(maxTextBox.Text);

            if (max < min)
            {
                minTextBox.Text = max.ToString();
            }

            ChangeHappened();
        }

        private void btnMinUp_Click(object sender, RoutedEventArgs e)
        {
            // Get the name of the button that sent the click event and determine what "category" it is.
            var category = (sender as RepeatButton).Name.Replace("btn", "").Replace("MinUp", "");

            // Get controls/fields relevant to the category
            var minTextBox = (TextBox)this.FindName(String.Format("txt{0}Min", category));
            var maxTextBox = (TextBox)this.FindName(String.Format("txt{0}Max", category));

            // Check for null/not found controls/fields
            if (minTextBox == null || maxTextBox == null)
            {
                return;
            }

            var value = Convert.ToInt32(minTextBox.Text);
            value++; // Add 1 to value

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            // Make sure if min is greater than max to update max 
            if (value > Convert.ToInt32(maxTextBox.Text))
            {
                maxTextBox.Text = value.ToString();
            }

            // Update min
            minTextBox.Text = value.ToString();

            ChangeHappened();
        }

        private void btnMinDown_Click(object sender, RoutedEventArgs e)
        {
            // Get the name of the button that sent the click event and determine what "category" it is.
            var category = (sender as RepeatButton).Name.Replace("btn", "").Replace("MinDown", "");

            // Get controls/fields relevant to the category
            var minTextBox = (TextBox)this.FindName(String.Format("txt{0}Min", category));

            // Check for null/not found controls/fields
            if (minTextBox == null)
            {
                return;
            }

            var value = Convert.ToInt32(minTextBox.Text);
            value--; // Remove 1 from value

            if (value < 0)
            {
                value = 0;
            }

            // Update min
            minTextBox.Text = value.ToString();

            ChangeHappened();
        }

        private void btnMaxUp_Click(object sender, RoutedEventArgs e)
        {
            // Get the name of the button that sent the click event and determine what "category" it is.
            var category = (sender as RepeatButton).Name.Replace("btn", "").Replace("MaxUp", "");

            // Get controls/fields relevant to the category
            var maxTextBox = (TextBox)this.FindName(String.Format("txt{0}Max", category));

            // Check for null/not found controls/fields
            if (maxTextBox == null)
            {
                return;
            }

            var value = Convert.ToInt32(maxTextBox.Text);
            value++; // Add 1 to value

            if (value > Number_Maximum)
            {
                value = Number_Maximum;
            }

            // Update max
            maxTextBox.Text = value.ToString();

            ChangeHappened();
        }

        private void btnMaxDown_Click(object sender, RoutedEventArgs e)
        {
            // Get the name of the button that sent the click event and determine what "category" it is.
            var category = (sender as RepeatButton).Name.Replace("btn", "").Replace("MaxDown", "");

            // Get controls/fields relevant to the category
            var minTextBox = (TextBox)this.FindName(String.Format("txt{0}Min", category));
            var maxTextBox = (TextBox)this.FindName(String.Format("txt{0}Max", category));

            // Check for null/not found controls/fields
            if (minTextBox == null || maxTextBox == null)
            {
                return;
            }

            var value = Convert.ToInt32(maxTextBox.Text);
            value--; // Remove 1 from value

            if (value < 0)
            {
                value = 0;
            }

            // Make sure if max is less than min to update min
            if (value < Convert.ToInt32(minTextBox.Text))
            {
                minTextBox.Text = value.ToString();
            }

            // Update max
            maxTextBox.Text = value.ToString();

            ChangeHappened();
        }

        //================================================================================
        // Category Item Manipulation
        //================================================================================

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentContainer == null)
            {
                return;
            }

            // Get the name of the button that sent the click event and determine what "category" it is.
            var category = (sender as Button).Name.Replace("btn", "").Replace("Add", "");

            var grid = String.Format("data{0}", category); // Format = data"Category"
            var list = String.Format("{0}List", category); // Format = "Category"List
            var button = String.Format("btn{0}AddText", category); // Format = btn"Category"AddText

            // Get controls/fields relevant to the category
            var dataGrid = (DataGrid)this.FindName(grid);
            var addTextButton = (TextBlock)this.FindName(button);
            var itemList = (ObservableCollection<Item>)this.GetType().GetField(list, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

            // Find the Name, Value, and Description textboxes for updating data
            var txtName = (TextBox)this.FindName(String.Format("txt{0}Name", category));
            var txtValue = (TextBox)this.FindName(String.Format("txt{0}Value", category));
            var txtDescription = (TextBox)this.FindName(String.Format("txt{0}Description", category));

            // Check for null/not found controls/fields
            if (dataGrid == null || itemList == null || addTextButton == null || txtName == null && txtValue == null && txtDescription == null)
            {
                return;
            }

            // Check if we're adding or updating items
            if (addTextButton.Text.Equals("Add Item", StringComparison.CurrentCultureIgnoreCase))
            {
                AddItem(txtName, txtValue, txtDescription, itemList, dataGrid);
            }
            else
            { // Update item
                EditItem(txtName, txtValue, txtDescription, itemList, dataGrid, addTextButton);
            }

            UpdateAvailableCategoryItemAmounts();
        }

        private void AddItem(TextBox txtName, TextBox txtValue, TextBox txtDescription, ObservableCollection<Item> itemList, DataGrid dataGrid)
        {
            Item item = new Item();

            var name = (txtName.Text.Equals(String.Empty)) ? "No name provided" : txtName.Text;
            item.Name = name;

            item.Value = txtValue.Text;

            var desc = (txtDescription.Text.Equals(String.Empty)) ? "No description provided" : txtDescription.Text;
            item.Description = desc;

            itemList.Add(item);
            dataGrid.Items.Refresh();

            ChangeHappened();
        }

        private void EditItem(TextBox txtName, TextBox txtValue, TextBox txtDescription, ObservableCollection<Item> itemList, DataGrid dataGrid, TextBlock addTextButton)
        {
            // First make sure we are editing a item
            if (editedItem != null)
            {
                // Make sure the "editedItem" still exists in the list, if not add it instead of updating it.
                if (!itemList.Contains(editedItem))
                {
                    AddItem(txtName, txtValue, txtDescription, itemList, dataGrid);
                }
                else
                {
                    var name = (txtName.Text.Equals(String.Empty)) ? "No name provided" : txtName.Text;
                    editedItem.Name = name;

                    editedItem.Value = txtValue.Text;

                    var desc = (txtDescription.Text.Equals(String.Empty)) ? "No description provided" : txtDescription.Text;
                    editedItem.Description = desc;

                    dataGrid.Items.Refresh();
                }

                // Reset inputs
                txtName.Text = String.Empty;
                txtValue.Text = "0gp";
                txtDescription.Text = String.Empty;

                ChangeHappened();
            }

            addTextButton.Text = "Add Item";
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Get the name of the button that sent the click event and determine what "category" it is.
            var category = (sender as Button).Name.Replace("btn", "").Replace("Edit", "");

            var grid = String.Format("data{0}", category); // Format = data"Category"
            var list = String.Format("{0}List", category); // Format = "Category"List

            // Get controls/fields relevant to the category
            var dataGrid = (DataGrid)this.FindName(grid);
            var itemList = (ObservableCollection<Item>)this.GetType().GetField(list, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

            // Check for null/not found controls/fields
            if (dataGrid == null || itemList == null)
            {
                return;
            }

            // Make sure we have at least one item selected before continuing 
            if (dataGrid.Items.Count > 0)
            {
                // Check if popups are suppressed for container editing
                if (!programStorage.Settings.SuppressContainerEditPopups)
                {
                    var proceed = WarningPopup.Show("Edit Item?", "Editing the selected item will clear the existing input data.\n\nThe input cannot be recovered. Proceed?");
                    if (proceed != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                foreach (Item item in dataGrid.SelectedItems)
                {
                    foreach (Item listItem in itemList)
                    {
                        if (item.Name.Equals(listItem.Name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            editedItem = item;


                            // Find the Name, Value, and Description textboxes for updating data
                            var txtName = (TextBox)this.FindName(String.Format("txt{0}Name", category));
                            var txtValue = (TextBox)this.FindName(String.Format("txt{0}Value", category));
                            var txtDescription = (TextBox)this.FindName(String.Format("txt{0}Description", category));
                            var btnAddText = (TextBlock)this.FindName(String.Format("btn{0}AddText", category));

                            if (txtName != null && txtValue != null && txtDescription != null && btnAddText != null)
                            {
                                txtName.Text = item.Name;
                                txtValue.Text = item.Value;
                                txtDescription.Text = item.Description;

                                btnAddText.Text = "Update Item";

                                return; // Get only first selected item
                            }
                        }
                    }
                }
            }
        }

        private void btnDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // Get the name of the button that sent the click event and determine what "category" it is.
            var category = (sender as Button).Name.Replace("btn", "").Replace("Duplicate", "");

            var grid = String.Format("data{0}", category); // Format = data"Category"
            var list = String.Format("{0}List", category); // Format = "Category"List

            // Get controls/fields relevant to the category
            var dataGrid = (DataGrid)this.FindName(grid);
            var itemList = (ObservableCollection<Item>)this.GetType().GetField(list, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

            // Check for null/not found controls/fields
            if (dataGrid == null || itemList == null)
            {
                return;
            }

            // Loop through each selection, create new Item for each, and add to the item list
            foreach (Item item in dataGrid.SelectedItems)
            {
                Item newItem = new Item();
                newItem.Name = item.Name;
                newItem.Value = item.Value;
                newItem.Description = item.Description;

                itemList.Add(newItem);
            }

            dataGrid.Items.Refresh(); // Refresh the datagrid items
            UpdateAvailableCategoryItemAmounts();

            ChangeHappened();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            // Get the name of the button that sent the click event and determine what "category" it is.
            var category = (sender as Button).Name.Replace("btn", "").Replace("Remove", "");

            var grid = String.Format("data{0}", category); // Format = data"Category"
            var list = String.Format("{0}List", category); // Format = "Category"List

            // Get controls/fields relevant to the category
            var dataGrid = (DataGrid)this.FindName(grid);
            var itemList = (ObservableCollection<Item>)this.GetType().GetField(list, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

            // Check for null/not found controls/fields
            if (dataGrid == null || itemList == null)
            {
                return;
            }

            // Make sure we have at least one item selected before continuing 
            if (dataGrid.Items.Count > 0)
            {
                // Check if popups are suppressed for container editing
                if (!programStorage.Settings.SuppressContainerEditPopups)
                {
                    var proceed = WarningPopup.Show("Remove Selection?", "Are you sure you want to remove the selected item(s)?\n\nThe changes cannot be reverted. Proceed?");
                    if (proceed != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                ObservableCollection<Item> removeList = new ObservableCollection<Item>();
                // Loop through each selection and add them to a list of items to remove
                foreach (Item item in dataGrid.SelectedItems)
                {
                    foreach (Item listItem in itemList)
                    {
                        if (listItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            removeList.Add(item); // Add to the removeList to be removed while not iterating
                            break;
                        }
                    }
                }

                // Remove each item in the removeList from the itemList
                foreach (Item item in removeList)
                {
                    itemList.Remove(item);
                }

                dataGrid.Items.Refresh(); // Refresh the datagrid items

                UpdateAvailableCategoryItemAmounts();
                ChangeHappened();
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            // Get the name of the button that sent the click event and determine what "category" it is.
            var category = (sender as Button).Name.Replace("btn", "").Replace("Clear", "");

            var grid = String.Format("data{0}", category); // Format = data"Category"
            var list = String.Format("{0}List", category); // Format = "Category"List

            // Get controls/fields relevant to the category
            var dataGrid = (DataGrid)this.FindName(grid);
            var itemList = (ObservableCollection<Item>)this.GetType().GetField(list, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

            // Check for null/not found controls/fields
            if (dataGrid == null || itemList == null)
            {
                return;
            }

            // Make sure we have at least one item selected before continuing 
            if (dataGrid.Items.Count > 0)
            {
                // Check if popups are suppressed for container editing
                if (!programStorage.Settings.SuppressContainerEditPopups)
                {
                    var proceed = WarningPopup.Show("Clear Items?", "Are you sure you want to clear the item list?\n\nThe changes cannot be reverted. Proceed?");
                    if (proceed != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                // Remove all from the list
                itemList.Clear();
                dataGrid.Items.Refresh(); // Refresh the datagrid items

                UpdateAvailableCategoryItemAmounts();

                // Find and update the "Add Item" button text as a failsafe for when editing.
                var buttonText = String.Format("btn{0}AddText", category);
                var addItemButton = (TextBlock)this.FindName(buttonText);

                if (addItemButton != null)
                {
                    addItemButton.Text = "Add Item"; // Failsafe
                }

                ChangeHappened();
            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            // Get the name of the button that sent the click event and determine what "category" it is.
            var category = (sender as Button).Name.Replace("btn", "").Replace("Import", "");

            var grid = String.Format("data{0}", category); // Format = data"Category"
            var list = String.Format("{0}List", category); // Format = "Category"List

            // Get controls/fields relevant to the category
            var dataGrid = (DataGrid)this.FindName(grid);
            var itemList = (ObservableCollection<Item>)this.GetType().GetField(list, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            var comboBox = (ComboBox)this.FindName(String.Format("combo{0}Import", category));

            // Check for null/not found controls/fields
            if (dataGrid == null || itemList == null || comboBox == null)
            {
                return;
            }

            var text = comboBox.Text;

            if (text.Equals(String.Empty))
            {
                return;
            }

            // Search for matching container
            LootContainer container = null;
            foreach (LootContainer lootcontainer in programStorage.LootContainers)
            {
                if (lootcontainer.Name.Equals(text, StringComparison.CurrentCultureIgnoreCase))
                {
                    container = lootcontainer;
                    break;
                }
            }

            if (container == null)
            {
                return; // Selection is not valid container
            }

            List<Item> containerSet = (List<Item>)container.GetType().GetProperty(category).GetValue(container);
            if (containerSet == null)
            {
                return;  // containerSet for "Category" not found
            }

            // Copy all items from the "Category" from the selected container
            foreach (Item item in containerSet)
            {
                itemList.Add(item);
            }

            dataGrid.Items.Refresh(); // Refresh the datagrid items

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

        private void btnDuplicateContainer_Click(object sender, RoutedEventArgs e)
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
            if (currentContainer == null || !programStorage.LootContainers.Contains(currentContainer))
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
                if (File.Exists(endFile))
                {
                    File.Delete(endFile);
                }

                File.Move(startFile, endFile);

                programStorage.LootContainers.Remove(currentContainer);
                
                programStorage.GenerateLootContainerTypesLists();
                PopulateContainerTree();

                ClearControlInputs();
            }
        }

        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow options = new OptionsWindow();
            options.ShowDialog();
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

            // Check to see if the container to save appears to be a "default" new container
            if (currentContainer.Name.Equals("New Loot Container") && currentContainer.Type.Equals("No Type") && currentContainer.Description.Equals("Placeholder container description"))
            {
                var proceed = WarningPopup.Show("Save Container?", "The container you are attempting to save appears to be a default new loot container. Make sure you set a proper Name, Type, and Description. Proceed with saving?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
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

            currentContainer.MundaneMin = Convert.ToInt32(txtMundaneItemsMin.Text);
            currentContainer.MundaneMax = Convert.ToInt32(txtMundaneItemsMax.Text);

            currentContainer.TrinketMin = Convert.ToInt32(txtTrinketsMin.Text);
            currentContainer.TrinketMax = Convert.ToInt32(txtTrinketsMax.Text);

            currentContainer.ArmorSets = new List<Item>(ArmorSetsList);
            currentContainer.ArmorSetsMin = Convert.ToInt32(txtArmorSetsMin.Text);
            currentContainer.ArmorSetsMax = Convert.ToInt32(txtArmorSetsMax.Text);
            currentContainer.ArmorPieces = new List<Item>(ArmorPiecesList);
            currentContainer.ArmorPiecesMin = Convert.ToInt32(txtArmorPiecesMin.Text);
            currentContainer.ArmorPiecesMax = Convert.ToInt32(txtArmorPiecesMax.Text);
            currentContainer.Weapons = new List<Item>(WeaponsList);
            currentContainer.WeaponsMin = Convert.ToInt32(txtWeaponsMin.Text);
            currentContainer.WeaponsMax = Convert.ToInt32(txtWeaponsMax.Text);
            currentContainer.Ammo = new List<Item>(AmmoList);
            currentContainer.AmmoMin = Convert.ToInt32(txtAmmoMin.Text);
            currentContainer.AmmoMax = Convert.ToInt32(txtAmmoMax.Text);
            currentContainer.Clothing = new List<Item>(ClothingList);
            currentContainer.ClothingMin = Convert.ToInt32(txtClothingMin.Text);
            currentContainer.ClothingMax = Convert.ToInt32(txtClothingMax.Text);
            currentContainer.ClothingAccessories = new List<Item>(ClothingAccessoriesList);
            currentContainer.ClothingAccessoriesMin = Convert.ToInt32(txtClothingAccessoriesMin.Text);
            currentContainer.ClothingAccessoriesMax = Convert.ToInt32(txtClothingAccessoriesMax.Text);
            currentContainer.FoodDrinks = new List<Item>(FoodDrinksList);
            currentContainer.FoodDrinksMin = Convert.ToInt32(txtFoodDrinksMin.Text);
            currentContainer.FoodDrinksMax = Convert.ToInt32(txtFoodDrinksMax.Text);
            currentContainer.TradeGoods = new List<Item>(TradeGoodsList);
            currentContainer.TradeGoodsMin = Convert.ToInt32(txtTradeGoodsMin.Text);
            currentContainer.TradeGoodsMax = Convert.ToInt32(txtTradeGoodsMax.Text);
            currentContainer.PreciousItems = new List<Item>(PreciousItemsList);
            currentContainer.PreciousItemsMin = Convert.ToInt32(txtPreciousItemsMin.Text);
            currentContainer.PreciousItemsMax = Convert.ToInt32(txtPreciousItemsMax.Text);
            currentContainer.ArtDecor = new List<Item>(ArtDecorList);
            currentContainer.ArtDecorMin = Convert.ToInt32(txtArtDecorMin.Text);
            currentContainer.ArtDecorMax = Convert.ToInt32(txtArtDecorMax.Text);
            currentContainer.BooksPapers = new List<Item>(BooksPapersList);
            currentContainer.BooksPapersMin = Convert.ToInt32(txtBooksPapersMin.Text);
            currentContainer.BooksPapersMax = Convert.ToInt32(txtBooksPapersMax.Text);
            currentContainer.OtherItems = new List<Item>(OtherItemsList);
            currentContainer.OtherItemsMin = Convert.ToInt32(txtOtherItemsMin.Text);
            currentContainer.OtherItemsMax = Convert.ToInt32(txtOtherItemsMax.Text);

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

        private void UpdateAvailableCategoryItemAmounts()
        {
            lblTrinketItems.Content = programStorage.Trinkets.Count;
            lblMundaneItems.Content = programStorage.MundaneItems.Count;
            lblArmorSetsItems.Content = dataArmorSets.Items.Count;
            lblArmorPiecesItems.Content = dataArmorPieces.Items.Count;
            lblWeaponItems.Content = dataWeapons.Items.Count;
            lblAmmoItems.Content = dataAmmo.Items.Count;
            lblClothingItems.Content = dataClothing.Items.Count;
            lblClothingAccessoriesItems.Content = dataClothingAccessories.Items.Count;
            lblFoodDrinkItems.Content = dataFoodDrinks.Items.Count;
            lblTradeGoodsItems.Content = dataTradeGoods.Items.Count;
            lblPreciousItems.Content = dataPreciousItems.Items.Count;
            lblArtDecorItems.Content = dataArtDecor.Items.Count;
            lblBooksPapersItems.Content = dataBooksPapers.Items.Count;
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

            newContainer.TrinketMin = currentContainer.TrinketMin;
            newContainer.TrinketMax = currentContainer.TrinketMax;

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

            txtMundaneItemsMin.Text = "0";
            txtMundaneItemsMax.Text = "0";

            txtTrinketsMin.Text = "0";
            txtTrinketsMax.Text = "0";

            txtArmorSetsMin.Text = "0";
            txtArmorSetsMax.Text = "0";
            ArmorSetsList = new ObservableCollection<Item>();
            dataArmorSets.ItemsSource = ArmorSetsList;
            dataArmorSets.Items.Refresh();

            txtArmorPiecesMin.Text = "0";
            txtArmorPiecesMax.Text = "0";
            ArmorPiecesList = new ObservableCollection<Item>();
            dataArmorPieces.ItemsSource = ArmorPiecesList;
            dataArmorPieces.Items.Refresh();

            txtWeaponsMin.Text = "0";
            txtWeaponsMax.Text = "0";
            WeaponsList = new ObservableCollection<Item>();
            dataWeapons.ItemsSource = WeaponsList;
            dataWeapons.Items.Refresh();

            txtAmmoMin.Text = "0";
            txtAmmoMax.Text = "0";
            AmmoList = new ObservableCollection<Item>();
            dataAmmo.ItemsSource = AmmoList;
            dataAmmo.Items.Refresh();

            txtClothingMin.Text = "0";
            txtClothingMax.Text = "0";
            ClothingList = new ObservableCollection<Item>();
            dataClothing.ItemsSource = ClothingList;
            dataClothing.Items.Refresh();

            txtClothingAccessoriesMin.Text = "0";
            txtClothingAccessoriesMax.Text = "0";
            ClothingAccessoriesList = new ObservableCollection<Item>();
            dataClothingAccessories.ItemsSource = ClothingAccessoriesList;
            dataClothingAccessories.Items.Refresh();

            txtFoodDrinksMin.Text = "0";
            txtFoodDrinksMax.Text = "0";
            FoodDrinksList = new ObservableCollection<Item>();
            dataFoodDrinks.ItemsSource = FoodDrinksList;
            dataFoodDrinks.Items.Refresh();

            txtTradeGoodsMin.Text = "0";
            txtTradeGoodsMax.Text = "0";
            TradeGoodsList = new ObservableCollection<Item>();
            dataTradeGoods.ItemsSource = TradeGoodsList;
            dataTradeGoods.Items.Refresh();

            txtPreciousItemsMin.Text = "0";
            txtPreciousItemsMax.Text = "0";
            PreciousItemsList = new ObservableCollection<Item>();
            dataPreciousItems.ItemsSource = PreciousItemsList;
            dataPreciousItems.Items.Refresh();

            txtArtDecorMin.Text = "0";
            txtArtDecorMax.Text = "0";
            ArtDecorList = new ObservableCollection<Item>();
            dataArtDecor.ItemsSource = ArtDecorList;
            dataArtDecor.Items.Refresh();

            txtBooksPapersMin.Text = "0";
            txtBooksPapersMax.Text = "0";
            BooksPapersList = new ObservableCollection<Item>();
            dataBooksPapers.ItemsSource = BooksPapersList;
            dataBooksPapers.Items.Refresh();

            txtOtherItemsMin.Text = "0";
            txtOtherItemsMax.Text = "0";
            OtherItemsList = new ObservableCollection<Item>();
            dataOtherItems.ItemsSource = OtherItemsList;
            dataOtherItems.Items.Refresh();

            UpdateAvailableCategoryItemAmounts();
            currentContainerHasChanged = false;

            comboContainerType.ItemsSource = null;
            comboContainerType.ItemsSource = programStorage.ContainerTypes;

            containerNames = new List<string>();
            foreach (LootContainer container in programStorage.LootContainers)
            {
                containerNames.Add(container.Name);
            }

            comboArmorSetsImport.ItemsSource = null;
            comboArmorSetsImport.ItemsSource = containerNames;
            comboArmorSetsImport.Text = String.Empty;

            comboArmorPiecesImport.ItemsSource = null;
            comboArmorPiecesImport.ItemsSource = containerNames;
            comboArmorPiecesImport.Text = String.Empty;

            comboWeaponsImport.ItemsSource = null;
            comboWeaponsImport.ItemsSource = containerNames;
            comboWeaponsImport.Text = String.Empty;

            comboAmmoImport.ItemsSource = null;
            comboAmmoImport.ItemsSource = containerNames;
            comboAmmoImport.Text = String.Empty;

            comboClothingImport.ItemsSource = null;
            comboClothingImport.ItemsSource = containerNames;
            comboClothingImport.Text = String.Empty;

            comboClothingAccessoriesImport.ItemsSource = null;
            comboClothingAccessoriesImport.ItemsSource = containerNames;
            comboClothingAccessoriesImport.Text = String.Empty;

            comboFoodDrinksImport.ItemsSource = null;
            comboFoodDrinksImport.ItemsSource = containerNames;
            comboFoodDrinksImport.Text = String.Empty;

            comboTradeGoodsImport.ItemsSource = null;
            comboTradeGoodsImport.ItemsSource = containerNames;
            comboTradeGoodsImport.Text = String.Empty;

            comboPreciousItemsImport.ItemsSource = null;
            comboPreciousItemsImport.ItemsSource = containerNames;
            comboPreciousItemsImport.Text = String.Empty;

            comboArtDecorImport.ItemsSource = null;
            comboArtDecorImport.ItemsSource = containerNames;
            comboArtDecorImport.Text = String.Empty;

            comboBooksPapersImport.ItemsSource = null;
            comboBooksPapersImport.ItemsSource = containerNames;
            comboBooksPapersImport.Text = String.Empty;

            comboOtherItemsImport.ItemsSource = null;
            comboOtherItemsImport.ItemsSource = containerNames;
            comboOtherItemsImport.Text = String.Empty;
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
