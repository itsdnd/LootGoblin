using System;
using System.ComponentModel;
using System.Windows;

namespace LootGoblin
{
    /// <summary>
    /// Interaction logic for WarningPopup.xaml
    /// </summary>
    public partial class WarningPopup : Window
    {
        private static WarningPopup window;
        private static MessageBoxResult result;

        public WarningPopup()
        {
            InitializeComponent();
        }

        public static MessageBoxResult Show(string header, string message)
        {
            if (window == null)
            {
                window = new WarningPopup();
            }

            window.txtHeader.Text = String.Format("{0}", header);
            window.txtWarningText.Text = message;
            result = MessageBoxResult.No;
            window.ShowDialog();
            return result;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            this.Visibility = Visibility.Hidden;
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            this.Visibility = Visibility.Hidden;
        }
        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
    }
}
