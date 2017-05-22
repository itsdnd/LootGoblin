using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace LootGoblin
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private ProgramStorage programStorage;

        public OptionsWindow()
        {
            InitializeComponent();

            // Initialize needed variables
            programStorage = ProgramStorage.GetInstance();

            btnSuppressEncounterListPopups.IsChecked = programStorage.Settings.SuppressEncounterListPopups;
            btnSuppressMagicItemListPopups.IsChecked = programStorage.Settings.SuppressMagicItemListPopups;
            btnSuppressRandomMagicItemListPopups.IsChecked = programStorage.Settings.SuppressRandomMagicItemListPopups;
            btnSuppressContainerEditPopups.IsChecked = programStorage.Settings.SuppressContainerEditPopups;
            btnSuppressMagicItemEditPopups.IsChecked = programStorage.Settings.SuppressMagicItemEditPopups;
            btnSuppressExternalLinksPopups.IsChecked = programStorage.Settings.SuppressExternalLinksPopups;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            programStorage.Settings.SuppressEncounterListPopups = btnSuppressEncounterListPopups.IsChecked.Value;
            programStorage.Settings.SuppressMagicItemListPopups = btnSuppressMagicItemListPopups.IsChecked.Value;
            programStorage.Settings.SuppressRandomMagicItemListPopups = btnSuppressRandomMagicItemListPopups.IsChecked.Value;
            programStorage.Settings.SuppressContainerEditPopups = btnSuppressContainerEditPopups.IsChecked.Value;
            programStorage.Settings.SuppressMagicItemEditPopups = btnSuppressMagicItemEditPopups.IsChecked.Value;
            programStorage.Settings.SuppressExternalLinksPopups = btnSuppressExternalLinksPopups.IsChecked.Value;

            programStorage.Settings.Save();
            this.Close();
        }
    }
}
