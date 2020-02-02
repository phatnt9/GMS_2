using GateAccessControl.Views;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace GateAccessControl.ViewModels
{
    internal class LoginPageViewModel : ViewModelBase
    {
        public ICommand LoginCommand { get; set; }

        public LoginPageViewModel(Frame frame)
        {
            LoginCommand = new RelayCommand<string>(
                 (p) =>
                 {
                     if (!String.IsNullOrEmpty(LoginName) && !String.IsNullOrEmpty(p))
                     {
                         return true;
                     }
                     else
                     {
                         return false;
                     }
                 },
                 (p) =>
                 {
                     Login(p, frame);
                 });
        }

        private void Login(string pass, Frame frame)
        {
            if (LoginName.ToLower().Equals("admin") && pass.ToLower().Equals("atek"))
            {
                GlobalConstant.CreateFolderToSaveData();
                SqliteDataAccess.CreateDatabase();
                frame.Content = new AppPage();
            }
        }

        private string _loginName;

        public String LoginName
        {
            get => _loginName;
            set
            {
                _loginName = value;
                RaisePropertyChanged("LoginName");
            }
        }
    }
}