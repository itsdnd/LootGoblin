using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace LootGoblin
{
    /// <summary>
    /// Interaction logic for MundaneItemsWindow.xaml
    /// </summary>
    public partial class MundaneItemsWindow : Window
    {
        ProgramStorage programStorage;

        public MundaneItemsWindow()
        {
            InitializeComponent();
            programStorage = ProgramStorage.GetInstance();

            loadItems();
        }

        private void loadItems()
        {
            txtMundaneItems.Text = String.Empty;
            foreach(var item in programStorage.MundaneItems)
            {
                var line = String.Format("{0}{1}", item, Environment.NewLine);
                txtMundaneItems.AppendText(line);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Get/Set Path
            string path = Path.Combine(Environment.CurrentDirectory, @"config\");
            Directory.CreateDirectory(path);

            List<string> items = new List<string>();
            for(int i = 0; i < txtMundaneItems.LineCount; i++)
            {
                var line = txtMundaneItems.GetLineText(i);
                if (!line.Trim().Equals(String.Empty))
                {
                    items.Add(line.Trim());
                }
            }

            programStorage.MundaneItems = items; // Direct the mundane item list to our new list
            loadItems(); // Refresh textbox

            // Notify of save successful
            WarningPopup.Show("Save Successful", "The mundane item list has been updated successfully.");

            // Write to file
            File.WriteAllLines(Path.Combine(path, "mundaneitems.txt"), programStorage.MundaneItems);
        }
    }
}
