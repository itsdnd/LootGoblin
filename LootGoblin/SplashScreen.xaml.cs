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

                if (container == null)
                {
                    continue;
                }

                // Make sure the container is not missing any details
                CheckNullContainer(container, files[i]);

                programStorage.LootContainers.Add(container);

                // Update progress bar
                var progressString = String.Format("Loading '{0}'...", Path.GetFileNameWithoutExtension(Path.Combine(dataPath, files[i])));
                var progress = ((i / (double)totalFiles) * 100);
                progress = (progress < 5) ? 5 : progress;
                progress = (progress > 75) ? 75 : progress;

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
                progressBar.Value = 80;
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
                progressBar.Value = 90;
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

            //================================================================================
            // Trinkets
            //================================================================================

            this.Dispatcher.BeginInvoke((Action)delegate () {
                txtStatus.Text = "Loading Trinkets...";
                progressBar.Value = 95;
            });

            var trinketFile = Path.Combine(configPath, "trinkets.txt");
            if (File.Exists(trinketFile))
            {
                var input = File.ReadAllLines(trinketFile);
                foreach (var item in input)
                {
                    if (item.Trim().Equals(String.Empty))
                    {
                        continue; // Skip if blank line
                    }
                    programStorage.Trinkets.Add(item.Trim());
                }

                File.WriteAllLines(trinketFile, programStorage.Trinkets);
            }

            Thread.Sleep(500); 

            this.Dispatcher.BeginInvoke((Action)delegate () {
                progressBar.Value = 100;
            });
        }

        private void CheckNullContainer(LootContainer container, string fileName)
        {
            var change = false;
            if (container.Name == null || container.Name.Trim().Equals(String.Empty))
            {
                container.Name = Path.GetFileNameWithoutExtension(fileName);
                change = true;
            }

            if (container.Type == null || container.Type.Trim().Equals(String.Empty))
            {
                container.Type = "No Type";
                change = true;
            }

            if (container.Description == null || container.Description.Trim().Equals(String.Empty))
            {
                container.Description = "Placeholder container description";
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
                item.Type = "No Type";
            }

            if (item.Rarity == null || item.Rarity.Trim().Equals(String.Empty))
            {
                item.Rarity = "No Rarity";
            }

            if (item.Description == null || item.Description.Trim().Equals(String.Empty))
            {
                item.Description = "Placeholder item description";
            }
        }
    }
}
