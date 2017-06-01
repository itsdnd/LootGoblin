using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LootGoblin.Controls
{
    /// <summary>
    /// Interaction logic for HeaderControl.xaml
    /// </summary>
    public partial class HeaderControl : UserControl
    {
        private ProgramStorage programStorage;

        public HeaderControl()
        {
            InitializeComponent();

            // Initialize needed variables
            programStorage = ProgramStorage.GetInstance();
        }

        private void btnWiki_Click(object sender, RoutedEventArgs e)
        {
            if (!programStorage.Settings.SuppressExternalLinksPopups)
            {
                var proceed = WarningPopup.Show("Go to Link?", "Clicking proceed will open up an external Internet browser on your computer that leads to the Loot Goblin wiki. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            Process.Start("https://github.com/ItsDnD/LootGoblin/wiki");
        }

        private void btnBug_Click(object sender, RoutedEventArgs e)
        {
            if (!programStorage.Settings.SuppressExternalLinksPopups)
            {
                var proceed = WarningPopup.Show("Go to Link?", "Clicking proceed will open up an external Internet browser on your computer that leads to the Loot Goblin Bug Report Page. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            Process.Start("https://github.com/ItsDnD/LootGoblin/wiki/Submitting-a-Bug-Report");
        }

        private void btnFeature_Click(object sender, RoutedEventArgs e)
        {
            if (!programStorage.Settings.SuppressExternalLinksPopups)
            {
                var proceed = WarningPopup.Show("Go to Link?", "Clicking proceed will open up an external Internet browser on your computer that leads to the Loot Goblin Feature Requests. Proceed?");
                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            Process.Start("https://github.com/ItsDnD/LootGoblin/wiki/Submitting-a-Feature-Request");
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow options = new OptionsWindow();
            options.ShowDialog();
        }
    }
}
