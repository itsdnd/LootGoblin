using System.Windows;

namespace LootGoblin
{
    /// <summary>
    /// Interaction logic for ContainerWindow.xaml
    /// </summary>
    public partial class ContainerWindow : Window
    {
        public ContainerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var currentWindowWidth = SystemParameters.PrimaryScreenWidth;
            var currentWindowHeight = SystemParameters.PrimaryScreenHeight;

            this.Width = (currentWindowWidth * 0.80);
            this.Height = (currentWindowHeight * 0.80);

            this.Left = (currentWindowWidth / 2) - (this.Width / 2);
            this.Top = (currentWindowHeight / 2) - (this.Height / 2) - 20;

            this.WindowState = WindowState.Maximized;
        }
    }
}
