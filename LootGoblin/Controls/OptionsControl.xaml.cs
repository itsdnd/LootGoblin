using System.Windows;
using System.Windows.Controls;

namespace LootGoblin.Controls
{
    /// <summary>
    /// Interaction logic for OptionsControl.xaml
    /// </summary>
    public partial class OptionsControl : UserControl
    {
        private ProgramStorage programStorage;

        public OptionsControl()
        {
            InitializeComponent();

            // Initialize needed variables
            programStorage = ProgramStorage.GetInstance();

            // Popup suppression
            btnSuppressEncounterListPopups.IsChecked = programStorage.Settings.SuppressEncounterListPopups;
            btnSuppressMagicItemListPopups.IsChecked = programStorage.Settings.SuppressMagicItemListPopups;
            btnSuppressRandomMagicItemListPopups.IsChecked = programStorage.Settings.SuppressRandomMagicItemListPopups;
            btnSuppressContainerEditPopups.IsChecked = programStorage.Settings.SuppressContainerEditPopups;
            btnSuppressMagicItemEditPopups.IsChecked = programStorage.Settings.SuppressMagicItemEditPopups;
            btnSuppressExternalLinksPopups.IsChecked = programStorage.Settings.SuppressExternalLinksPopups;

            // Mouse wheel scrolling override
            btnOverrideContainerListMouseWheelScrolling.IsChecked = programStorage.Settings.OverrideContainerListMouseWheelScrolling;
            btnOverrideMagicItemListMouseWheelScrolling.IsChecked = programStorage.Settings.OverrideMagicItemListMouseWheelScrolling;
            btnOverrideEncounterListMouseWheelScrolling.IsChecked = programStorage.Settings.OverrideEncounterListMouseWheelScrolling;
            btnOverrideGuaranteedMagicItemsMouseWheelScrolling.IsChecked = programStorage.Settings.OverrideGuaranteedMagicItemsMouseWheelScrolling;
            btnOverrideRandomMagicItemsMouseWheelScrolling.IsChecked = programStorage.Settings.OverrideRandomMagicItemsMouseWheelScrolling;
            btnOverrideContainerEditGridMouseWheelScrolling.IsChecked = programStorage.Settings.OverrideContainerEditGridMouseWheelScrolling;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Popup suppression
            programStorage.Settings.SuppressEncounterListPopups = btnSuppressEncounterListPopups.IsChecked.Value;
            programStorage.Settings.SuppressMagicItemListPopups = btnSuppressMagicItemListPopups.IsChecked.Value;
            programStorage.Settings.SuppressRandomMagicItemListPopups = btnSuppressRandomMagicItemListPopups.IsChecked.Value;
            programStorage.Settings.SuppressContainerEditPopups = btnSuppressContainerEditPopups.IsChecked.Value;
            programStorage.Settings.SuppressMagicItemEditPopups = btnSuppressMagicItemEditPopups.IsChecked.Value;
            programStorage.Settings.SuppressExternalLinksPopups = btnSuppressExternalLinksPopups.IsChecked.Value;

            // Mouse wheel scrolling override
            programStorage.Settings.OverrideContainerListMouseWheelScrolling = btnOverrideContainerListMouseWheelScrolling.IsChecked.Value;
            programStorage.Settings.OverrideMagicItemListMouseWheelScrolling = btnOverrideMagicItemListMouseWheelScrolling.IsChecked.Value;
            programStorage.Settings.OverrideEncounterListMouseWheelScrolling = btnOverrideEncounterListMouseWheelScrolling.IsChecked.Value;
            programStorage.Settings.OverrideGuaranteedMagicItemsMouseWheelScrolling = btnOverrideGuaranteedMagicItemsMouseWheelScrolling.IsChecked.Value;
            programStorage.Settings.OverrideRandomMagicItemsMouseWheelScrolling = btnOverrideRandomMagicItemsMouseWheelScrolling.IsChecked.Value;
            programStorage.Settings.OverrideContainerEditGridMouseWheelScrolling = btnOverrideContainerEditGridMouseWheelScrolling.IsChecked.Value;

            programStorage.Settings.Save();
            Window.GetWindow(this).Close();
        }
    }
}
