using System;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.ComponentModel;
using LootGoblin.Storage;
using LootGoblin.Controls;

namespace LootGoblin
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        private MainControl mainControl;
        private ProgramStorage programStorage;
        private BackgroundWorker backgroundWorker = new BackgroundWorker();

        public SplashScreen(MainControl mainControl)
        {
            InitializeComponent();
            txtStatus.Text = String.Empty; // Clear starting text
            
            this.mainControl = mainControl;
            programStorage = ProgramStorage.GetInstance();

            backgroundWorker.DoWork += DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
            mainControl.Initialize();
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(500); 

            //================================================================================
            // Verify the needed directories exist
            //================================================================================
            string dataPath = Path.Combine(Environment.CurrentDirectory, @"containers\");
            Directory.CreateDirectory(dataPath);
            string configPath = Path.Combine(Environment.CurrentDirectory, @"config\");
            Directory.CreateDirectory(configPath);
            string encounterPath = Path.Combine(Environment.CurrentDirectory, @"encounters\");
            Directory.CreateDirectory(encounterPath);
            string outputPath = Path.Combine(Environment.CurrentDirectory, @"loot\");
            Directory.CreateDirectory(outputPath);

            this.Dispatcher.BeginInvoke((Action)delegate () {
                progressBar.Value = 0;
                txtStatus.Text = "Loading Settings...";
            });

            Thread.Sleep(500); 

            //================================================================================
            // Verify the needed directories exist
            //================================================================================

            var filePath = Path.Combine(configPath, "config.json");
            if (File.Exists(filePath))
            {
                string input = File.ReadAllText(filePath);
                programStorage.Settings = JsonConvert.DeserializeObject<AppSettings>(input);
            }
            else
            {
                programStorage.Settings = new AppSettings();
            }

            Thread.Sleep(500); 

            //================================================================================
            // Loot Containers
            //================================================================================

            var files = Directory.GetFiles(dataPath);
            var totalFiles = files.Length;
            for (var i = 0; i < totalFiles; i++)
            {
                // Load file & deserialize
                var input = File.ReadAllText(files[i]);
                var container = JsonConvert.DeserializeObject<LootContainer>(input);

                // Make sure the container is not missing any details
                CheckNullContainer(container, files[i]);

                programStorage.LootContainers.Add(container);

                // Update progress bar
                var progressString = String.Format("Loading '{0}'...", Path.GetFileNameWithoutExtension(Path.Combine(dataPath, files[i])));
                var progress = ((i / (double)totalFiles) * 100);
                progress = (progress < 5) ? 5 : progress;
                progress = (progress > 90) ? 90 : progress;

                this.Dispatcher.BeginInvoke((Action)delegate () {
                    txtStatus.Text = progressString;
                    progressBar.Value = progress;
                });

                Thread.Sleep(50); 
            }

            Thread.Sleep(500); 

            //================================================================================
            // Magic Items
            //================================================================================

            this.Dispatcher.BeginInvoke((Action)delegate () {
                txtStatus.Text = "Loading Magic Items...";
                progressBar.Value = 95;
            });
            
            var magicFile = Path.Combine(configPath, "magicitems.json");
            if (File.Exists(magicFile))
            {
                var input = File.ReadAllText(magicFile);
                programStorage.MagicItems = JsonConvert.DeserializeObject<List<MagicItem>>(input);

                // Make sure each magic item is not missing any details
                foreach (var item in programStorage.MagicItems)
                {
                    CheckNullItem(item);
                }

                // Resave file with updated information
                var output = JsonConvert.SerializeObject(programStorage.MagicItems, Formatting.Indented);
                File.WriteAllText(magicFile, output);
            }

            Thread.Sleep(500); 

            //================================================================================
            // Mundane Items
            //================================================================================

            this.Dispatcher.BeginInvoke((Action)delegate () {
                txtStatus.Text = "Loading Mundane Items...";
                progressBar.Value = 100;
            });

            var mundaneFile = Path.Combine(configPath, "mundaneitems.txt");
            if (File.Exists(mundaneFile))
            {
                var input = File.ReadAllLines(mundaneFile);
                foreach (var item in input)
                {
                    if (item.Trim().Equals(String.Empty))
                    {
                        continue; // Skip if blank line
                    }
                    programStorage.MundaneItems.Add(item.Trim());
                }

                File.WriteAllLines(mundaneFile, programStorage.MundaneItems);
            }

            Thread.Sleep(500); 

            this.Dispatcher.BeginInvoke((Action)delegate () {
                progressBar.Value = 100;
            });
        }

        private void CheckNullContainer(LootContainer container, string fileName)
        {
            // Potentially reduce IO by only resaving when changes occur?
            var change = false;

            if (container.Name == null || container.Name.Trim().Equals(String.Empty))
            {
                container.Name = Path.GetFileNameWithoutExtension(fileName);
                change = true;
            }

            if (container.Type == null || container.Type.Trim().Equals(String.Empty))
            {
                container.Type = "Other";
                change = true;
            }

            if (container.Description == null || container.Description.Trim().Equals(String.Empty))
            {
                container.Description = "Placeholder container description";
                change = true;
            }

            if (container.CopperMin < 0)
            {
                container.CopperMin = 0;
                change = true;
            }

            if (container.CopperMax < 0)
            {
                container.CopperMax = 0;
                change = true;
            }

            if (container.SilverMin < 0)
            {
                container.SilverMin = 0;
                change = true;
            }

            if (container.SilverMax < 0)
            {
                container.SilverMax = 0;
                change = true;
            }

            if (container.ElectrumMin < 0)
            {
                container.ElectrumMin = 0;
                change = true;
            }

            if (container.ElectrumMax < 0)
            {
                container.ElectrumMax = 0;
                change = true;
            }

            if (container.GoldMin < 0)
            {
                container.GoldMin = 0;
                change = true;
            }

            if (container.GoldMax < 0)
            {
                container.GoldMax = 0;
                change = true;
            }

            if (container.PlatinumMin < 0)
            {
                container.PlatinumMin = 0;
                change = true;
            }

            if (container.PlatinumMax < 0)
            {
                container.PlatinumMax = 0;
                change = true;
            }

            if (container.ArmorMin < 0)
            {
                container.ArmorMin = 0;
                change = true;
            }

            if (container.ArmorMax < 0)
            {
                container.ArmorMax = 0;
                change = true;
            }

            if (container.Armor == null)
            {
                container.Armor = new List<Item>();
                change = true;
            }

            if (container.WeaponsMin < 0)
            {
                container.WeaponsMin = 0;
                change = true;
            }

            if (container.WeaponsMax < 0)
            {
                container.WeaponsMax = 0;
                change = true;
            }

            if (container.Weapons == null)
            {
                container.Weapons = new List<Item>();
                change = true;
            }

            if (container.GearMin < 0)
            {
                container.GearMin = 0;
                change = true;
            }

            if (container.GearMax < 0)
            {
                container.GearMax = 0;
                change = true;
            }

            if (container.Gear == null)
            {
                container.Gear = new List<Item>();
                change = true;
            }

            if (container.MundaneMin < 0)
            {
                container.MundaneMin = 0;
                change = true;
            }

            if (container.MundaneMax < 0)
            {
                container.MundaneMax = 0;
                change = true;
            }

            if (change)
            {
                var output = JsonConvert.SerializeObject(container, Formatting.Indented);
                File.WriteAllText(fileName, output);
            }
        }

        public void CheckNullItem(MagicItem item)
        {
            if (item.Name == null || item.Name.Trim().Equals(String.Empty))
            {
                item.Name = "Placeholder Magic Item Name";
            }

            if (item.Type == null || item.Type.Trim().Equals(String.Empty))
            {
                item.Type = "Wondrous";
            }

            if (item.Rarity == null || item.Rarity.Trim().Equals(String.Empty))
            {
                item.Rarity = "Common";
            }

            if (item.Description == null || item.Description.Trim().Equals(String.Empty))
            {
                item.Description = "Placeholder item description";
            }
        }
    }
}
