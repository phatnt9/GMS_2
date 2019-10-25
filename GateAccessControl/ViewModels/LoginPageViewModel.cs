using GateAccessControl.Views;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace GateAccessControl.ViewModels
{
    class LoginPageViewModel : ViewModelBase
    {
        public ICommand LoginCommand { get; set; }

        public LoginPageViewModel(Frame frame)
        {
            LoginCommand = new RelayCommand<string>(
                 (p) =>
                 {
                     if (!String.IsNullOrEmpty(LoginName) && !String.IsNullOrEmpty(p))
                         return true;
                     else
                         return false;
                 },
                 (p) =>
                 {
                     Login(p,frame);
                 });
        }

        private void Login(string pass, Frame frame)
        {
            if(LoginName.ToLower().Equals("admin") && pass.ToLower().Equals("atek"))
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
