using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

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
