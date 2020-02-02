using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace GateAccessControl.Views
{
    /// <summary>
    /// Interaction logic for AppPage.xaml
    /// </summary>
    public partial class AppPage : Page
    {
        public AppPage()
        {
            InitializeComponent();
            DataContext = new AppPageViewModel();
        }

        private void Btn_openDatabase_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK");
        }
    }
}