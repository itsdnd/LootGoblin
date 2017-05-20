using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace LootGoblin
{
    /// <summary>
    /// Interaction logic for TrinketsWindow.xaml
    /// </summary>
    public partial class TrinketsWindow : Window
    {
        ProgramStorage programStorage;

        public TrinketsWindow()
        {
            InitializeComponent();
            programStorage = ProgramStorage.GetInstance();

            loadItems();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var currentWindowWidth = SystemParameters.PrimaryScreenWidth;
            var currentWindowHeight = SystemParameters.PrimaryScreenHeight;

            this.Width = (currentWindowWidth * 0.60);
            this.Height = (currentWindowHeight * 0.55);

            this.Left = (currentWindowWidth / 2) - (this.Width / 2);
            this.Top = (currentWindowHeight / 2) - (this.Height / 2) - 20;
        }

        private void loadItems()
        {
            txtTrinkets.Text = String.Empty;
            foreach (var item in programStorage.Trinkets)
            {
                var line = String.Format("{0}{1}", item, Environment.NewLine);
                txtTrinkets.AppendText(line);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Get/Set Path
            string path = Path.Combine(Environment.CurrentDirectory, @"config\");
            Directory.CreateDirectory(path);

            List<string> items = new List<string>();
            for (int i = 0; i < txtTrinkets.LineCount; i++)
            {
                var line = txtTrinkets.GetLineText(i);
                if (!line.Trim().Equals(String.Empty))
                {
                    items.Add(line.Trim());
                }
            }

            programStorage.Trinkets = items; // Direct the trinket list to our new list
            loadItems(); // Refresh textbox

            // Notify of save successful
            WarningPopup.Show("Save Successful", "The trinkets list has been updated successfully.");

            // Write to file
            File.WriteAllLines(Path.Combine(path, "trinkets.txt"), programStorage.Trinkets);
        }
    }
}
