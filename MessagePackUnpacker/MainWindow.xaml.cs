﻿using System.Windows;

namespace MessagePackUnpacker
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}
