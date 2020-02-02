using System.Windows;

namespace GateAccessControl.Views
{
    /// <summary>
    /// Interaction logic for EditDeviceWindow.xaml
    /// </summary>
    public partial class EditDeviceWindow : Window
    {
        public EditDeviceWindow(Device p)
        {
            InitializeComponent();
            DataContext = new EditDeviceViewModels(p);
        }
    }
}