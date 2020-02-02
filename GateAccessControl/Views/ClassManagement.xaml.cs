using System.Windows;

namespace GateAccessControl.Views
{
    /// <summary>
    /// Interaction logic for ClassManagement.xaml
    /// </summary>
    public partial class ClassManagement : Window
    {
        public ClassManagement()
        {
            InitializeComponent();
            DataContext = new ClassManagementViewModel();
        }
    }
}