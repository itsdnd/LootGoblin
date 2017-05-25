using System;
using System.Windows;

namespace LootGoblin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WindowState = WindowState.Minimized;
            this.Visibility = Visibility.Hidden;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var currentWindowWidth = SystemParameters.PrimaryScreenWidth;
            var currentWindowHeight = SystemParameters.PrimaryScreenHeight;

            this.Width = (currentWindowWidth * 0.85);
            this.Height = (currentWindowHeight * 0.85);

            this.Left = (currentWindowWidth / 2) - (this.Width / 2);
            this.Top = (currentWindowHeight / 2) - (this.Height / 2) - 20;

            this.WindowState = WindowState.Maximized;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
