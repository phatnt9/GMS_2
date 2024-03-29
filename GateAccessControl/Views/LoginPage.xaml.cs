﻿using GateAccessControl.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GateAccessControl.Views
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        Frame frame;
        public LoginPage(Frame frame)
        {
            this.frame = frame;
            InitializeComponent();
            Loaded += LoginPage_Loaded;
            //DataContext = new LoginPageViewModel(frame);
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            tb_userName.Focus();
            tb_userName.SelectAll();
        }

        private void Btn_login_Click(object sender, RoutedEventArgs e)
        {
            if(tb_userName.Text == "admin" && tb_password.Password == "atek")
            {
                GlobalConstant.CreateFolderToSaveData();
                SqliteDataAccess.CreateDatabase();
                frame.Content = new AppPage();
            }
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
