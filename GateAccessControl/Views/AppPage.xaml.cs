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

        private void Btn_createTable_Click(object sender, RoutedEventArgs e)
        {
            //SqliteDataAccess.CreateDeviceProfilesTable("TESTTEST");
        }

        private void Btn_openDatabase_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK");
        }
    }
}
