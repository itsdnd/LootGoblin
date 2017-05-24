using LootGoblin.Storage;
using LootGoblin.Storage.Grids;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LootGoblin.Controls
{
    /// <summary>
    /// Interaction logic for OutputControl.xaml
    /// </summary>
    public partial class OutputControl : UserControl
    {
        private ProgramStorage programStorage;
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        Random random;

        private int copper = 0;
        private int silver = 0;
        private int electrum = 0;
        private int gold = 0;
        private int platinum = 0;

        private List<Loot> lootList = new List<Loot>();

        private Dictionary<MagicItem, int> randomMagicItemList = new Dictionary<MagicItem, int>();
        private Dictionary<MagicItem, int> guaranteedMagicItemList = new Dictionary<MagicItem, int>();
        private Dictionary<string, int> mundaneListEncounter = new Dictionary<string, int>();
        private Dictionary<string, int> trinketListEncounter = new Dictionary<string, int>();

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

        private bool first = true;

        public OutputControl()
        {
            InitializeComponent();

            // Initialize needed variables
            programStorage = ProgramStorage.GetInstance();
            random = new Random();

            backgroundWorker.DoWork += DoWork;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            OutputGuaranteedMagicItems();
            GenerateRandomMagicItems();
            GenerateLoot();
            OutputEncounterLoot();
        }

        private void OutputGuaranteedMagicItems()
        {
            if (programStorage.MagicItemList.Count == 0)
            {
                AppendText(txtGuaranteedMagicItems, "No Guaranteed Magic Items");
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
                var description = (!pair.Key.Description.Equals(String.Empty)) ? String.Format(" - {0}", pair.Key.Description) : "";
                var itemString = String.Format("- {0}{1} [{2} | {3}]{4}{5}{6}", amount, pair.Key.Name, pair.Key.Type, pair.Key.Rarity, description, Environment.NewLine, Environment.NewLine);
                AppendText(txtGuaranteedMagicItems, itemString);
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
                AppendText(txtRandomMagicItems, "No Random Magic Items");
                return;
            }

            foreach (KeyValuePair<MagicItem, int> pair in randomMagicItemList)
            {
                var amount = (pair.Value > 1) ? String.Format("[x{0}] ", pair.Value) : "";
                var description = (!pair.Key.Description.Equals(String.Empty)) ? String.Format(" - {0}", pair.Key.Description) : "";
                var itemString = String.Format("- {0}{1} [{2} | {3}]{4}{5}{6}", amount, pair.Key.Name, pair.Key.Type, pair.Key.Rarity, description, Environment.NewLine, Environment.NewLine);
                
                AppendText(txtRandomMagicItems, itemString);
            }
        }

        private void GenerateLoot()
        {
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
                        if (first)
                        {
                            ClearGearBoxes();
                            first = false;
                        }

                        GenerateContainerLoot(container);
                    }
                }
            }
        }

        private void ClearGearBoxes()
        {
            this.Dispatcher.BeginInvoke((Action)delegate () {
                txtIndividualGear.Text = String.Empty;
                txtEncounterGear.Text = String.Empty;
            }, DispatcherPriority.Background);
        }

        private void GenerateContainerLoot(LootContainer container)
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

            if (programStorage.Trinkets.Count > 0)
            {
                if (container.TrinketMax > 0)
                {
                    var count = (container.TrinketMin < container.TrinketMax) ? random.Next(container.TrinketMin, container.TrinketMax + 1) : container.TrinketMax;
                    Dictionary<string, int> temp = new Dictionary<string, int>();

                    for (int j = 1; j <= count; j++)
                    {
                        var selection = programStorage.Trinkets[random.Next(0, programStorage.Trinkets.Count)];
                        if (temp.ContainsKey(selection))
                        {
                            temp[selection] += 1;
                        }
                        else
                        {
                            temp.Add(selection, 1);
                        }

                        if (trinketListEncounter.ContainsKey(selection))
                        {
                            trinketListEncounter[selection] += 1;
                        }
                        else
                        {
                            trinketListEncounter.Add(selection, 1);
                        }
                    }

                    loot.Trinkets = temp;
                }
            }

            loot.ArmorSets = GenerateContainerLoot(container.ArmorSets, container.ArmorSetsMin, container.ArmorSetsMax, armorSetListEncounter);
            loot.ArmorPieces = GenerateContainerLoot(container.ArmorPieces, container.ArmorPiecesMin, container.ArmorPiecesMax, armorPiecesListEncounter);
            loot.Weapons = GenerateContainerLoot(container.Weapons, container.WeaponsMin, container.WeaponsMax, weaponListEncounter);
            loot.Ammo = GenerateContainerLoot(container.Ammo, container.AmmoMin, container.AmmoMax, ammoListEncounter);
            loot.Clothing = GenerateContainerLoot(container.Clothing, container.ClothingMin, container.ClothingMax, clothingListEncounter);
            loot.ClothingAccessories = GenerateContainerLoot(container.ClothingAccessories, container.ClothingAccessoriesMin, container.ClothingAccessoriesMax, clothingAccessoriesListEncounter);
            loot.FoodDrinks = GenerateContainerLoot(container.FoodDrinks, container.FoodDrinksMin, container.FoodDrinksMax, foodDrinksListEncounter);
            loot.TradeGoods = GenerateContainerLoot(container.TradeGoods, container.TradeGoodsMin, container.TradeGoodsMax, tradeGoodsListEncounter);
            loot.PreciousItems = GenerateContainerLoot(container.PreciousItems, container.PreciousItemsMin, container.PreciousItemsMax, preciousItemsListEncounter);
            loot.ArtDecor = GenerateContainerLoot(container.ArtDecor, container.ArtDecorMin, container.ArtDecorMax, artDecorListEncounter);
            loot.BooksPapers = GenerateContainerLoot(container.BooksPapers, container.BooksPapersMin, container.BooksPapersMax, booksPapersListEncounter);
            loot.OtherItems = GenerateContainerLoot(container.OtherItems, container.OtherItemsMin, container.OtherItemsMax, otherItemsListEncounter);

            OutputLoot(loot);
        }

        private Dictionary<Item, int> GenerateContainerLoot(List<Item> list, int min, int max, Dictionary<Item, int> encounterList)
        {
            Dictionary<Item, int> temp = new Dictionary<Item, int>();
            
            if (list.Count > 0)
            {
                if (max > 0)
                {
                    var count = (min < max) ? random.Next(min, max + 1) : max;
                    for (int i = 1; i <= count; i++)
                    {
                        var selection = list[random.Next(0, list.Count)];
                        if (temp.ContainsKey(selection))
                        {
                            temp[selection] += 1;
                        }
                        else
                        {
                            temp.Add(selection, 1);
                        }

                        if (encounterList.ContainsKey(selection))
                        {
                            encounterList[selection] += 1;
                        }
                        else
                        {
                            encounterList.Add(selection, 1);
                        }
                    }
                }
            }

            return temp;
        }

        private void OutputLoot(Loot loot)
        {
            AppendText(txtIndividualGear, String.Format("{0}:{1}", loot.Name, Environment.NewLine));
            AppendText(txtIndividualGear, String.Format("Copper: {0} | Silver: {1} | Electrum: {2} | Gold: {3} | Platinum: {4} {5}", loot.Copper, loot.Silver, loot.Electrum, loot.Gold, loot.Platinum, Environment.NewLine));

            OutputItems("Armor Sets", loot.ArmorSets);
            OutputItems("Armor Pieces", loot.ArmorPieces);
            OutputItems("Weapons", loot.Weapons);
            OutputItems("Ammmo", loot.Ammo);
            OutputItems("Clothing", loot.Clothing);
            OutputItems("Clothing Accessories", loot.ClothingAccessories);
            OutputItems("Food & Drinks", loot.FoodDrinks);
            OutputItems("Trade Goods", loot.TradeGoods);
            OutputItems("Precious Items", loot.PreciousItems);
            OutputItems("Art & Decor", loot.ArtDecor);
            OutputItems("Books & Papers", loot.BooksPapers);
            OutputItems("Other Items", loot.OtherItems);

            if (loot.Trinkets != null && loot.Trinkets.Count > 0)
            {
                AppendText(txtIndividualGear, Environment.NewLine + "Trinkets:" + Environment.NewLine);
                foreach (KeyValuePair<string, int> pair in loot.Trinkets)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    AppendText(txtIndividualGear, String.Format(" - {0}{1}{2}", amount, pair.Key, Environment.NewLine));
                }
            }

            if (loot.MundaneItems != null && loot.MundaneItems.Count > 0)
            {
                AppendText(txtIndividualGear, Environment.NewLine + "Mundane Items:" + Environment.NewLine);
                foreach (KeyValuePair<string, int> pair in loot.MundaneItems)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    AppendText(txtIndividualGear, String.Format(" - {0}{1}{2}", amount, pair.Key, Environment.NewLine));
                }
            }

            string dashes = "------------------------------------------------------------------------------------------------------------------------------";
            AppendText(txtIndividualGear, String.Format("{0}{1}{2}{3}", Environment.NewLine, dashes, Environment.NewLine, Environment.NewLine));
        }

        private void OutputItems(string header, Dictionary<Item, int> dictionary)
        {
            if (dictionary != null && dictionary.Count > 0)
            {
                AppendText(txtIndividualGear, String.Format("{0}{1}:{2}", Environment.NewLine, header, Environment.NewLine));
                foreach (KeyValuePair<Item, int> pair in dictionary)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    var each = (pair.Value > 1) ? " ea." : "";
                    var value = (!pair.Key.Value.Equals(String.Empty)) ? String.Format(" ({0}{1})", pair.Key.Value, each) : "";
                    var description = (!pair.Key.Description.Equals(String.Empty)) ? String.Format(" - {0}", pair.Key.Description) : "";
                    AppendText(txtIndividualGear, String.Format(" - {0}{1}{2}{3}{4}", amount, pair.Key.Name, value, description, Environment.NewLine));
                }
            }
        }

        private void OutputEncounterLoot()
        {
            this.Dispatcher.BeginInvoke((Action)delegate () {
                txtCopper.Text = copper.ToString();
                txtSilver.Text = silver.ToString();
                txtElectrum.Text = electrum.ToString();
                txtGold.Text = gold.ToString();
                txtPlatinum.Text = platinum.ToString();
            }, DispatcherPriority.Background);

            OutputEncounterItems("Armor Sets", armorSetListEncounter);
            OutputEncounterItems("Armor Pieces", armorPiecesListEncounter);
            OutputEncounterItems("Weapons", weaponListEncounter);
            OutputEncounterItems("Ammmo", ammoListEncounter);
            OutputEncounterItems("Clothing", clothingListEncounter);
            OutputEncounterItems("Clothing Accessories", clothingAccessoriesListEncounter);
            OutputEncounterItems("Food & Drinks", foodDrinksListEncounter);
            OutputEncounterItems("Trade Goods", tradeGoodsListEncounter);
            OutputEncounterItems("Precious Items", preciousItemsListEncounter);
            OutputEncounterItems("Art & Decor", artDecorListEncounter);
            OutputEncounterItems("Books & Papers", booksPapersListEncounter);
            OutputEncounterItems("Other Items", otherItemsListEncounter);

            if (trinketListEncounter.Count > 0)
            {

                AppendText(txtEncounterGear, "Trinkets:" + Environment.NewLine);
                foreach (KeyValuePair<string, int> pair in trinketListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    AppendText(txtEncounterGear, String.Format(" - {0}{1}{2}", amount, pair.Key, Environment.NewLine));
                }
            }

            if (mundaneListEncounter.Count > 0)
            {

                AppendText(txtEncounterGear, "Mundane Items:" + Environment.NewLine);
                foreach (KeyValuePair<string, int> pair in mundaneListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    AppendText(txtEncounterGear, String.Format(" - {0}{1}{2}", amount, pair.Key, Environment.NewLine));
                }
            }
        }

        private void OutputEncounterItems(string header, Dictionary<Item, int> dictionary)
        {
            if (dictionary.Count > 0)
            {
                AppendText(txtEncounterGear, String.Format("{0}:{1}", header, Environment.NewLine));
                foreach (KeyValuePair<Item, int> pair in dictionary)
                {
                    var amount = (pair.Value > 1) ? String.Format("[{0}x] ", pair.Value) : "";
                    var each = (pair.Value > 1) ? " ea." : "";
                    var value = (!pair.Key.Value.Equals(String.Empty)) ? String.Format(" ({0}{1})", pair.Key.Value, each) : "";
                    var description = (!pair.Key.Description.Equals(String.Empty)) ? String.Format(" - {0}", pair.Key.Description) : "";
                    AppendText(txtEncounterGear, String.Format(" - {0}{1}{2}{3}{4}", amount, pair.Key.Name, value, description, Environment.NewLine));
                }

                string dashes = "------------------------------------------------------------------------------------------------------------------------------";
                AppendText(txtEncounterGear, String.Format("{0}{1}{2}{3}", Environment.NewLine, dashes, Environment.NewLine, Environment.NewLine));
            }
        }

        private void AppendText(TextBox textbox, string line)
        {
            this.Dispatcher.BeginInvoke((Action)delegate () {
                textbox.AppendText(line);
            }, DispatcherPriority.Background);
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
