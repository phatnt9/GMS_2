using System.Windows;

namespace GateAccessControl.Views
{
    /// <summary>
    /// Interaction logic for AddProfileWindow.xaml
    /// </summary>
    public partial class AddProfileWindow : Window
    {
        public AddProfileWindow()
        {
            InitializeComponent();
            DataContext = new AddProfileViewModel();
        }
    }
}