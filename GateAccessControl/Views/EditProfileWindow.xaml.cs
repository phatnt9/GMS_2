using System.Windows;

namespace GateAccessControl.Views
{
    /// <summary>
    /// Interaction logic for EditProfileWindow.xaml
    /// </summary>
    public partial class EditProfileWindow : Window
    {
        public EditProfileWindow(Profile p)
        {
            InitializeComponent();
            DataContext = new EditProfileViewModel(p);
        }
    }
}