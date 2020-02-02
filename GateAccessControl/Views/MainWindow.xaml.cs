using System;
using System.Windows;

namespace GateAccessControl.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            mainFrame.Content = new LoginPage(mainFrame);
        }
    }
}