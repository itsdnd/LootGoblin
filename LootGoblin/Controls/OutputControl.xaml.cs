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

        private Dictionary<Item, int> armorListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> weaponListEncounter = new Dictionary<Item, int>();
        private Dictionary<Item, int> gearListEncounter = new Dictionary<Item, int>();
        private Dictionary<string, int> mundaneListEncounter = new Dictionary<string, int>();
        private Dictionary<MagicItem, int> randomMagicItemList = new Dictionary<MagicItem, int>();
        private Dictionary<MagicItem, int> guaranteedMagicItemList = new Dictionary<MagicItem, int>();

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
                var amount = (pair.Value > 1) ? String.Format(" [x{0}]", pair.Value) : "";
                var itemString = String.Format("{0} ({1} | {2}){3} - {4}{5}{6}", pair.Key.Name, pair.Key.Type, pair.Key.Rarity, amount, pair.Key.Description, Environment.NewLine, Environment.NewLine);
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
                var amount = (pair.Value > 1) ? String.Format(" [x{0}]", pair.Value) : "";
                var itemString = String.Format("{0} ({1} | {2}){3} - {4}{5}{6}", pair.Key.Name, pair.Key.Type, pair.Key.Rarity, amount, pair.Key.Description, Environment.NewLine, Environment.NewLine);
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

                        if (container.Armor.Count > 0)
                        {
                            if (container.ArmorMax > 0)
                            {
                                var armorCount = (container.ArmorMin < container.ArmorMax) ? random.Next(container.ArmorMin, container.ArmorMax + 1) : container.ArmorMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= armorCount; j++)
                                {
                                    var selection = container.Armor[random.Next(0, container.Armor.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                        armorListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                        armorListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.Armor = temp;
                            }
                        }

                        if (container.Weapons.Count > 0)
                        {
                            if (container.WeaponsMax > 0)
                            {
                                var weaponCount = (container.WeaponsMin < container.WeaponsMax) ? random.Next(container.WeaponsMin, container.WeaponsMax + 1) : container.WeaponsMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= weaponCount; j++)
                                {
                                    var selection = container.Weapons[random.Next(0, container.Weapons.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                        weaponListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                        weaponListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.Weapons = temp;
                            }
                        }

                        if (container.Gear.Count > 0)
                        {
                            if (container.GearMax > 0)
                            {
                                var gearCount = (container.GearMin < container.GearMax) ? random.Next(container.GearMin, container.GearMax + 1) : container.GearMax;
                                Dictionary<Item, int> temp = new Dictionary<Item, int>();

                                for (int j = 1; j <= gearCount; j++)
                                {
                                    var selection = container.Gear[random.Next(0, container.Gear.Count)];
                                    if (temp.ContainsKey(selection))
                                    {
                                        temp[selection] += 1;
                                        gearListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                        gearListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.Gear = temp;
                            }

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
                                        mundaneListEncounter[selection] += 1;
                                    }
                                    else
                                    {
                                        temp.Add(selection, 1);
                                        mundaneListEncounter.Add(selection, 1);
                                    }
                                }

                                loot.MundaneItems = temp;
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
                txtIndividualGear.AppendText(Environment.NewLine);

                if (loot.Armor != null && loot.Armor.Count > 0)
                {
                    txtIndividualGear.AppendText("Armor:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.Armor)
                    {
                        var amount = (pair.Value > 1) ? String.Format(" [x{0}]", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", pair.Key.Name, amount, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.Weapons != null && loot.Weapons.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Weapons:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.Weapons)
                    {
                        var amount = (pair.Value > 1) ? String.Format(" [x{0}]", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", pair.Key.Name, amount, pair.Key.Description, Environment.NewLine));
                    }
                } 
                
                if (loot.Gear != null && loot.Gear.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Gear:" + Environment.NewLine);
                    foreach (KeyValuePair<Item, int> pair in loot.Gear)
                    {
                        var amount = (pair.Value > 1) ? String.Format(" [x{0}]", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1} - {2}{3}", pair.Key.Name, amount, pair.Key.Description, Environment.NewLine));
                    }
                }

                if (loot.MundaneItems != null && loot.MundaneItems.Count > 0)
                {
                    txtIndividualGear.AppendText(Environment.NewLine + "Mundane Items:" + Environment.NewLine);
                    foreach (KeyValuePair<string, int> pair in loot.MundaneItems)
                    {
                        var amount = (pair.Value > 1) ? String.Format(" [x{0}]", pair.Value) : "";
                        txtIndividualGear.AppendText(String.Format(" - {0}{1}{2}", pair.Key, amount, Environment.NewLine));
                    }
                    txtIndividualGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
                }
            }

            //================================================================================
            // Encounter Loot Output
            //================================================================================
            txtCopper.Text = copper.ToString();
            txtSilver.Text = silver.ToString();
            txtElectrum.Text = electrum.ToString();
            txtGold.Text = gold.ToString();
            txtPlatinum.Text = platinum.ToString();

            if (armorListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Armor:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in armorListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format(" [x{0}]", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", pair.Key.Name, amount, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (weaponListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Weapons:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in weaponListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format(" [x{0}]", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", pair.Key.Name, amount, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (gearListEncounter.Count > 0)
            {
                txtEncounterGear.AppendText("Gear:" + Environment.NewLine);
                foreach (KeyValuePair<Item, int> pair in gearListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format(" [x{0}]", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1} - {2}{3}", pair.Key.Name, amount, pair.Key.Description, Environment.NewLine));
                }
                txtEncounterGear.AppendText(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }

            if (mundaneListEncounter.Count > 0)
            {

                txtEncounterGear.AppendText("Mundane Items:" + Environment.NewLine);
                foreach (KeyValuePair<string, int> pair in mundaneListEncounter)
                {
                    var amount = (pair.Value > 1) ? String.Format(" [x{0}]", pair.Value) : "";
                    txtEncounterGear.AppendText(String.Format(" - {0}{1}{2}", pair.Key, amount, Environment.NewLine));
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
