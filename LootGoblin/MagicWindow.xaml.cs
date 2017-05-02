﻿using System;
using System.Windows;

namespace LootGoblin
{
    /// <summary>
    /// Interaction logic for MagicWindow.xaml
    /// </summary>
    public partial class MagicWindow : Window
    {
        public MagicWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var currentWindowWidth = SystemParameters.PrimaryScreenWidth;
            var currentWindowHeight = SystemParameters.PrimaryScreenHeight;

            this.Width = (currentWindowWidth * 0.55);
            this.Height = (currentWindowHeight * 0.80);

            this.Left = (currentWindowWidth / 2) - (this.Width / 2);
            this.Top = (currentWindowHeight / 2) - (this.Height / 2) - 20;
        }
    }
}
