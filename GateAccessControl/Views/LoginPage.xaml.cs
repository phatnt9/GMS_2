using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GateAccessControl.Views
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private Frame frame;

        public LoginPage(Frame frame)
        {
            this.frame = frame;
            InitializeComponent();
            Loaded += LoginPage_Loaded;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            tb_userName.Focus();
            tb_userName.SelectAll();
        }

        private void Btn_login_Click(object sender, RoutedEventArgs e)
        {
            if (Login(tb_userName.Text, tb_password.Password))
            {
                //GlobalConstant.CreateFolderToSaveData();
                //SqliteDataAccess.CreateDatabase();
                frame.Content = new AppPage();
            }
        }

        public bool Login(string userName, string password)
        {
            if (tb_userName.Text == "admin" && tb_password.Password == "atek")
            {
                return true;
            }
            return false;
        }

        private void Tb_password_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Btn_login_Click(sender, e);
            }
        }

        private void Tb_userName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb_password.Focus();
                tb_password.SelectAll();
            }
        }
    }
}