using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GateAccessControl.Views;

namespace GateAccessControl
{
    public class AppPageViewModel : ViewModelBase
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ObservableCollection<Profile> _profiles = new ObservableCollection<Profile>(SqliteDataAccess.LoadAllProfiles());
        private ObservableCollection<CardType> _classes = new ObservableCollection<CardType>(SqliteDataAccess.LoadAllCardType());
        public ObservableCollection<Profile> Profiles => _profiles;
        public ObservableCollection<CardType> Classes => _classes;


        public ICommand ImportProfilesCommand { get; set; }
        public ICommand ClassManagementCommand { get; set; }

        public AppPageViewModel()
        {
            ImportProfilesCommand = new RelayCommand<Profile>(
                (p) => true,
                (p) => 
                {
                    ImportProfiles();
                });

            ClassManagementCommand = new RelayCommand<CardType>(
                (p) => true,
                (p) =>
                {
                    ManageClass();
                });
        }

        public void ManageClass()
        {
            ClassManagement classManagement = new ClassManagement();
            classManagement.ShowDialog();
        }

        public void ImportProfiles()
        {
            ImportWindow importWindow = new ImportWindow();
            importWindow.ShowDialog();
        }

        public void ReloadDataProfiles(string name = "", string pinno = "", string adno = "")
        {
            try
            {
                List<Profile> profileList = SqliteDataAccess.LoadAllProfiles(name, pinno, adno);
                if (profileList.Count > 0)
                {
                    Profiles.Clear();
                    foreach (Profile item in profileList)
                    {
                        Profiles.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }
    }
}
