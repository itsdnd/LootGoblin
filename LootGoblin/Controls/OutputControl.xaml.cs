using LootGoblin.Storage;
using LootGoblin.Storage.Grids;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LootGoblin.Controls
{
    /// <summary>
    /// Interaction logic for OutputControl.xaml
    /// </summary>
    public partial class OutputControl : UserControl
    {
        private ProgramStorage programStorage;

        private int copper = 0;
        private int silver = 0;
        private int electrum = 0;
        private int gold = 0;
        private int platinum = 0;

        private List<Loot> lootList = new List<Loot>();

        private Dictionary<MagicItem, int> randomMagicItemList = new Dictionary<MagicItem, int>();
        private Dictionary<MagicItem, int> guaranteedMagicItemList = new Dictionary<MagicItem, int>();
        private Dictionary<string, int> mundaneListEncounter = new Dictionary<string, int>();

        private Dictionary<Item, int> armorSetListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> armorPiecesListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> weaponListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> ammoListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> clothingListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> clothingAccessoriesListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> foodDrinksListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> tradeGoodsListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> preciousItemsListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> artDecorListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> booksPapersListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> otherItemsListEncounter = new Dictionary<Item, int>();

        public OutputControl()
        {
            InitializeComponent();

            // Initialize needed variables
            programStorage = ProgramStorage.GetInstance();

            OutputGuaranteedMagicItems();
            GenerateRandomMagicItems();
            GenerateLoot();
            OutputLoot();
        }

        private void OutputGuaranteedMagicItems()
        {
            if (programStorage.MagicItemList.Count == 0)
            {
                txtGuaranteedMagicItems.Text = "No Guaranteed Magic Items";
                return;
            }

            foreach (MagicItem item in programStorage.MagicItemList)
            {
                if (guaranteedMagicItemList.ContainsKey(item))
                {
                    guaranteedMagicItemList[item] += 1;
                }
                else
                {
                    guaranteedMagicItemList.Add(item, 1);
                }
            }

            foreach (KeyValuePair<MagicItem, int> pair in guaranteedMagicItemList)
            {
                var amount = (pair.Value > 1) ? String.Format("[x{0}] ", pair.Value) : "";
                var itemString = String.Format("- {0}{1} ({2} | {3}) - {4}{5}{6}", amount, pair.Key.Name, pair.Key.Type, pair.Key.Rarity, pair.Key.Description, Environment.NewLine, Environment.NewLine);
                txtGuaranteedMagicItems.AppendText(itemString);
            }
        }

        private void GenerateRandomMagicItems()
        {
            Random random = new Random();
            foreach (RandomMagicItem randomItem in programStorage.RandomMagicItemList)
            {
                var min = randomItem.Min;
                var max = randomItem.Max;

                if (max == 0)
                {
                    continue; // Skip random rarities with max value of 0
                }

                List<MagicItem> temp = new List<MagicItem>();
                foreach (MagicItem item in programStorage.MagicItems)
                {
                    // Random Type, but set Rarity
                    if (randomItem.Type.Equals("Random") && !randomItem.Rarity.Equals("Random"))
                    {
                        if (item.Rarity.Equals(randomItem.Rarity, StringComparison.CurrentCultureIgnoreCase))
                        {
                            temp.Add(item);
                        }
                    } 
                    // Random Rarity, but set Type
                    else if (!randomItem.Type.Equals("Random") && randomItem.Rarity.Equals("Random"))
                    {
                        if (item.Type.Equals(randomItem.Type, StringComparison.CurrentCultureIgnoreCase))
                        {
                            temp.Add(item);
                        }
                    } 
                    // Random Type and Rarity
                    else if (randomItem.Type.Equals("Random") && randomItem.Rarity.Equals("Random"))
                    {
                        temp.Add(item);
                    }
                    // Set Type and Rarity
                    else
                    {
                        if (item.Type.Equals(randomItem.Type, StringComparison.CurrentCultureIgnoreCase) && item.Rarity.Equals(randomItem.Rarity, StringComparison.CurrentCultureIgnoreCase))
                        {
                            temp.Add(item);
                        }
                    }
                }

                if (temp.Count == 0)
                {
                    continue;
                }

                if (min > max) min = max;
                int amount = random.Next(min, max + 1);
                for (int i = 1; i <= amount; i++)
                {
                    var selection = temp[random.Next(0, temp.Count)];
                    if (randomMagicItemList.ContainsKey(selection))
                    {
                        randomMagicItemList[selection] += 1;
                    }
                    else
                    {
                        randomMagicItemList.Add(selection, 1);
                    }
                }
            }

            if (randomMagicItemList.Count == 0)
            {
                txtRandomMagicItems.Text = "No Random Magic Items";
                return;
            }

            foreach (KeyValuePair<MagicItem, int> pair in randomMagicItemList)
            {
                var amount = (pair.Value > 1) ? String.Format("[x{0}] ", pair.Value) : "";
                var itemString = String.Format("- {0}{1} ({2} | {3}) - {4}{5}{6}", amount, pair.Key.Name, pair.Key.Type, pair.Key.Rarity, pair.Key.Description, Environment.NewLine, Environment.NewLine);
                txtRandomMagicItems.AppendText(itemString);
            }
        }

        private void GenerateLoot()
        {
            Random random = new Random();
            foreach (EncounterContainer encounterContainer in programStorage.EncounterList)
            {
                LootContainer container = null;
                foreach (LootContainer lcontainer in programStorage.LootContainers)
                {
                    if (lcontainer.Name.Equals(encounterContainer.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        container = lcontainer;
                        break;
                    }
                }

                if (container != null)
                {
                    for (int i = 1; i <= encounterContainer.Quantity; i++)
                    {
                        Loot loot = new Loot();
                        loot.Name = container.Name;

                        if (container.CopperMax > 0)
                        {
                            loot.Copper = (container.CopperMin < container.CopperMax) ? random.Next(container.CopperMin, container.CopperMax + 1) : container.CopperMax;
                            copper += loot.Copper;
                        }

                        if (container.SilverMax > 0)
                        {
                            loot.Silver = (container.SilverMin < container.SilverMax) ? random.Next(container.SilverMin, container.SilverMax + 1) : container.SilverMax;
                            silver += loot.Silver;
                        }

                        if (container.ElectrumMax > 0)
                        {
                            loot.Electrum = (container.ElectrumMin < container.ElectrumMax) ? random.Next(container.ElectrumMin, container.ElectrumMax + 1) : container.ElectrumMax;
                            electrum += loot.Electrum;
                        }

                        if (container.GoldMax > 0)
                        {
                            loot.Gold = (container.GoldMin < container.GoldMax) ? random.Next(container.GoldMin, container.GoldMax + 1) : container.GoldMax;
                            gold += loot.Gold;
                        }

                        if (container.PlatinumMax > 0)
                        {
                            loot.Platinum = (container.PlatinumMin < container.PlatinumMax) ? random.Next(container.PlatinumMin, container.PlatinumMax + 1) : container.PlatinumMax;
                            platinum += loot.Platinum;
                        }

                        if (programStorage.MundaneItems.Count > 0)
                        {
                            if (container.MundaneMax > 0)
                            {
                                var mundaneCount = (container.MundaneMin < container.MundaneMax) ? random.Next(container.MundaneMin, container.MundaneMax + 1) : container.MundaneMax;
                                Dictionary<string, int> temp = new Dictionary<string, int>();

                                for (int j = 1; j <= mundaneCount; j++)
                                {
                                    var selection = programStorage.MundaneItems[random.Next(0, programStorage.MundaneItems.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }
                                    
                                    if (mundaneListEncounter.ContainsKey(selection))
                                    {
                                        mundaneListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        mundaneListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.MundaneItems = temp;
                            }
                        }

                        if (container.ArmorSets.Count > 0)
                        {
                            if (container.ArmorSetsMax > 0)
                            {
                                var count = (container.ArmorSetsMin < container.ArmorSetsMax) ? random.Next(container.ArmorSetsMin, container.ArmorSetsMax + 1) : container.ArmorSetsMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.ArmorSets[random.Next(0, container.ArmorSets.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }

                                    if (armorSetListEncounter.ContainsKey(selection))
                                    {
                                        armorSetListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        armorSetListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.ArmorSets = temp;
                            }
                        }

                        if (container.ArmorPieces.Count > 0)
                        {
                            if (container.ArmorPiecesMax > 0)
                            {
                                var count = (container.ArmorPiecesMin < container.ArmorPiecesMax) ? random.Next(container.ArmorPiecesMin, container.ArmorPiecesMax + 1) : container.ArmorPiecesMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.ArmorPieces[random.Next(0, container.ArmorPieces.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }

                                    if (armorPiecesListEncounter.ContainsKey(selection))
                                    {
                                        armorPiecesListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        armorPiecesListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.ArmorPieces = temp;
                            }
                        }

                        if (container.Weapons.Count > 0)
                        {
                            if (container.WeaponsMax > 0)
                            {
                                var count = (container.WeaponsMin < container.WeaponsMax) ? random.Next(container.WeaponsMin, container.WeaponsMax + 1) : container.WeaponsMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.Weapons[random.Next(0, container.Weapons.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }

                                    if (weaponListEncounter.ContainsKey(selection))
                                    {
                                        weaponListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        weaponListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.Weapons = temp;
                            }
                        }

                        if (container.Ammo.Count > 0)
                        {
                            if (container.AmmoMax > 0)
                            {
                                var count = (container.AmmoMin < container.AmmoMax) ? random.Next(container.AmmoMin, container.AmmoMax + 1) : container.AmmoMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.Ammo[random.Next(0, container.Ammo.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }

                                    if (ammoListEncounter.ContainsKey(selection))
                                    {
                                        ammoListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        ammoListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.Ammo = temp;
                            }
                        }

                        if (container.Clothing.Count > 0)
                        {
                            if (container.ClothingMax > 0)
                            {
                                var count = (container.ClothingMin < container.ClothingMax) ? random.Next(container.ClothingMin, container.ClothingMax + 1) : container.ClothingMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.Clothing[random.Next(0, container.Clothing.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }

                                    if (clothingListEncounter.ContainsKey(selection))
                                    {
                                        clothingListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        clothingListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.Clothing = temp;
                            }
                        }

                        if (container.ClothingAccessories.Count > 0)
                        {
                            if (container.ClothingAccessoriesMax > 0)
                            {
                                var count = (container.ClothingAccessoriesMin < container.ClothingAccessoriesMax) ? random.Next(container.ClothingAccessoriesMin, container.ClothingAccessoriesMax + 1) : container.ClothingAccessoriesMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.ClothingAccessories[random.Next(0, container.ClothingAccessories.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }

                                    if (clothingAccessoriesListEncounter.ContainsKey(selection))
                                    {
                                        clothingAccessoriesListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        clothingAccessoriesListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.ClothingAccessories = temp;
                            }
                        }

                        if (container.FoodDrinks.Count > 0)
                        {
                            if (container.FoodDrinksMax > 0)
                            {
                                var count = (container.FoodDrinksMin < container.FoodDrinksMax) ? random.Next(container.FoodDrinksMin, container.FoodDrinksMax + 1) : container.FoodDrinksMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.FoodDrinks[random.Next(0, container.FoodDrinks.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }

                                    if (foodDrinksListEncounter.ContainsKey(selection))
                                    {
                                        foodDrinksListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        foodDrinksListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.FoodDrinks = temp;
                            }
                        }

                        if (container.TradeGoods.Count > 0)
                        {
                            if (container.TradeGoodsMax > 0)
                            {
                                var count = (container.TradeGoodsMin < container.TradeGoodsMax) ? random.Next(container.TradeGoodsMin, container.TradeGoodsMax + 1) : container.TradeGoodsMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.TradeGoods[random.Next(0, container.TradeGoods.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }

                                    if (tradeGoodsListEncounter.ContainsKey(selection))
                                    {
                                        tradeGoodsListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        tradeGoodsListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.TradeGoods = temp;
                            }
                        }

                        if (container.PreciousItems.Count > 0)
                        {
                            if (container.PreciousItemsMax > 0)
                            {
                                var count = (container.PreciousItemsMin < container.PreciousItemsMax) ? random.Next(container.PreciousItemsMin, container.PreciousItemsMax + 1) : container.PreciousItemsMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.PreciousItems[random.Next(0, container.PreciousItems.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }
                                    
                                    if (preciousItemsListEncounter.ContainsKey(selection))
                                    {
                                        preciousItemsListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        preciousItemsListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.PreciousItems = temp;
                            }
                        }

                        if (container.ArtDecor.Count > 0)
                        {
                            if (container.ArtDecorMax > 0)
                            {
                                var count = (container.ArtDecorMin < container.ArtDecorMax) ? random.Next(container.ArtDecorMin, container.ArtDecorMax + 1) : container.ArtDecorMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.ArtDecor[random.Next(0, container.ArtDecor.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }

                                    if (artDecorListEncounter.ContainsKey(selection))
                                    {
                                        artDecorListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        artDecorListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.ArtDecor = temp;
                            }
                        }

                        if (container.BooksPapers.Count > 0)
                        {
                            if (container.BooksPapersMax > 0)
                            {
                                var count = (container.BooksPapersMin < container.BooksPapersMax) ? random.Next(container.BooksPapersMin, container.BooksPapersMax + 1) : container.BooksPapersMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.BooksPapers[random.Next(0, container.BooksPapers.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }

                                    if (booksPapersListEncounter.ContainsKey(selection))
                                    {
                                        booksPapersListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        booksPapersListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.BooksPapers = temp;
                            }
                        }

                        if (container.OtherItems.Count > 0)
                        {
                            if (container.OtherItemsMax > 0)
                            {
                                var count = (container.OtherItemsMin < container.OtherItemsMax) ? random.Next(container.OtherItemsMin, container.OtherItemsMax + 1) : container.OtherItemsMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= count; j++)
                                {
                                    var selection = container.OtherItems[random.Next(0, container.OtherItems.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                    }

                                    if (otherItemsListEncounter.ContainsKey(selection))
                                    {
                                        otherItemsListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        otherItemsListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.OtherItems = temp;
                            }

                        }

                        lootList.Add(loot);
                    }
                }
            }
        }
        
        private void OutputLoot()
        {
            if (lootList.Count == 0)
            {
                txtIndividualGear.Text = "No gear or items";
                txtEncounterGear.Text = "No gear or items";
                return;
            }

            //================================================================================
            // Individual Loot Output
            //================================================================================
            foreach (Loot loot in lootList)
            {
                txtIndividualGear.AppendText(String.Format("{0}:{1}", loot.Name, Environment.NewLine));
                txtIndividualGear.AppendText(String.Format("Copper: {0} | Silver: {1} | Electrum: {2} | Gold: {3} | Platinum: {4} {5}", loot.Copper, loot.Silver, loot.Electrum, loot.Gold, loot.Platinum, Environment.NewLine));

                if (loot.ArmorSets != null && loot.ArmorSets.Count > 0)
                {
                    txtIndividualGear.AppendText("Armor Sets:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.ArmorSets)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.ArmorPieces != null && loot.ArmorPieces.Count > 0)
                {
                    txtIndividualGear.AppendText("Armor Pieces:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.ArmorPieces)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.Weapons != null && loot.Weapons.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Weapons:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.Weapons)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.Ammo != null && loot.Ammo.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Ammo:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.Ammo)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.Clothing != null && loot.Clothing.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Clothing:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.Clothing)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.ClothingAccessories != null && loot.ClothingAccessories.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Clothing Accessories:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.ClothingAccessories)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.FoodDrinks != null && loot.FoodDrinks.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Food Drinks:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.FoodDrinks)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.TradeGoods != null && loot.TradeGoods.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Trade Goods:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.TradeGoods)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.PreciousItems != null && loot.PreciousItems.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Precious Items:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.PreciousItems)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.ArtDecor != null && loot.ArtDecor.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Art Decor:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.ArtDecor)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.BooksPapers != null && loot.BooksPapers.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Books Papers:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.BooksPapers)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.OtherItems != null && loot.OtherItems.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Other Items:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.OtherItems)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.MundaneItems != null && loot.MundaneItems.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Mundane Items:" + Environment.NewLine);
                    foreach (KeyValuePair<string, int> pair in loot.MundaneItems)
                    {
                        var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1}{2}", amount, pair.Key, Environment.NewLine));
                    }
                }

                txtIndividualGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            //================================================================================
            // Encounter Loot Output
            //================================================================================
            txtCopper.Text = copper.ToString();
            txtSilver.Text = silver.ToString();
            txtElectrum.Text = electrum.ToString();
            txtGold.Text = gold.ToString();
            txtPlatinum.Text = platinum.ToString();

            if (armorSetListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Armor Sets:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in armorSetListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (armorPiecesListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Armor Pieces:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in armorPiecesListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (weaponListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Weapons:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in weaponListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (ammoListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Ammo:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in ammoListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (clothingListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Clothing:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in clothingListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (clothingAccessoriesListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Clothing Accessories:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in clothingAccessoriesListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (foodDrinksListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Food & Drinks:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in foodDrinksListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (tradeGoodsListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Trade Goods:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in tradeGoodsListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (preciousItemsListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Precious Items:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in preciousItemsListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (artDecorListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Art & Decor:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in artDecorListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (booksPapersListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Books & Papers:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in booksPapersListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (otherItemsListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Other Items:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in otherItemsListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", amount, pair.Key.Name, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (mundaneListEncounter.Count > 0)
            {

                txtEncounterGear.AppendText("Mundane Items:" + Environment.NewLine);
                foreach (KeyValuePair<string, int> pair in mundaneListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1}{2}", amount, pair.Key, Environment.NewLine));
                }
            }
        }

        private void btnExportIndividual_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();

            string outputPath = Path.Combine(Environment.CurrentDirectory, @"loot\");
            Directory.CreateDirectory(outputPath);

            saveDialog.InitialDirectory = outputPath;
            saveDialog.RestoreDirectory = true;
            saveDialog.FileName = "loot(individual).txt";
            saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (saveDialog.ShowDialog() == true)
            {
                string output = String.Format("Guaranteed Magic Items:\n{0}\n\nRandom Magic Items:\n{1}\n\nGear & Items:\n{2}", txtGuaranteedMagicItems.Text, txtRandomMagicItems.Text, txtIndividualGear.Text);
                File.WriteAllText(saveDialog.FileName, output);
            }
        }

        private void btnExportEncounter_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();

            string outputPath = Path.Combine(Environment.CurrentDirectory, @"loot\");
            Directory.CreateDirectory(outputPath);

            saveDialog.InitialDirectory = outputPath;
            saveDialog.RestoreDirectory = true;
            saveDialog.FileName = "loot(encounter).txt";
            saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (saveDialog.ShowDialog() == true)
            {
                string currency = String.Format("Copper: {0}\nSilver: {1}\nElectrum: {2}\nGold: {3}\nPlatinum: {4}", txtCopper.Text, txtSilver.Text, txtElectrum.Text, txtGold.Text, txtPlatinum.Text);
                string output = String.Format("Currency:\n{0}\n\nGuaranteed Magic Items:\n{1}\n\nRandom Magic Items:\n{2}\n\nGear & Items:\n{3}", currency, txtGuaranteedMagicItems.Text, txtRandomMagicItems.Text, txtEncounterGear.Text);
                File.WriteAllText(saveDialog.FileName, output);
            }
        }
    }
}
