using System.Windows;

namespace GateAccessControl.Views
{
    /// <summary>
    /// Interaction logic for DeviceProfilesManagement.xaml
    /// </summary>
    public partial class DeviceProfilesManagement : Window
    {
        public DeviceProfilesManagement(Device d)
        {
            InitializeComponent();
            DataContext = new DeviceProfilesManageViewModel(d);
        }
    }
}