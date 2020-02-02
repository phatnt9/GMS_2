using System.Windows;

namespace GateAccessControl.Views
{
    /// <summary>
    /// Interaction logic for AddDeviceWindow.xaml
    /// </summary>
    public partial class AddDeviceWindow : Window
    {
        public AddDeviceWindow()
        {
            InitializeComponent();
            DataContext = new AddDeviceViewModels();
        }
    }
}