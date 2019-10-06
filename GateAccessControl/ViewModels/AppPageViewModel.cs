using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Controls;
using GateAccessControl.Views;

namespace GateAccessControl
{
    public class AppPageViewModel : ViewModelBase
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ObservableCollection<Profile> _profiles = new ObservableCollection<Profile>();
        private ObservableCollection<Profile> _deviceProfiles = new ObservableCollection<Profile>();
        private ObservableCollection<Profile> _timeChecks = new ObservableCollection<Profile>();
        private ObservableCollection<CardType> _classes = new ObservableCollection<CardType>();

        public ObservableCollection<Profile> Profiles => _profiles;
        public ObservableCollection<Profile> DeviceProfiles => _deviceProfiles;
        public ObservableCollection<Profile> TimeChecks => _timeChecks;
        public ObservableCollection<CardType> Classes => _classes;

        private string _search_profiles_class;
        private string _search_profiles_group;
        private string _search_profiles_others;

        private string _search_deviceProfiles_class;
        private string _search_deviceProfiles_group;
        private string _search_deviceProfiles_others;

        public String Search_profiles_class
        {
            get { return _search_profiles_class; }
            set {
                _search_profiles_class = value;
                RaisePropertyChanged("Search_profiles_class"); }
        }
        public String Search_profiles_group
        {
            get { return _search_profiles_group; }
            set
            {
                _search_profiles_group = value;
                RaisePropertyChanged("Search_profiles_group");
            }
        }
        public String Search_profiles_others
        {
            get { return _search_profiles_others; }
            set
            {
                _search_profiles_others = value;
                RaisePropertyChanged("Search_profiles_others");
            }
        }
        public String Search_deviceProfiles_class
        {
            get { return _search_deviceProfiles_class; }
            set
            {
                _search_deviceProfiles_class = value;
                RaisePropertyChanged("Search_deviceProfiles_class");
            }
        }
        public String Search_deviceProfiles_group
        {
            get { return _search_deviceProfiles_group; }
            set
            {
                _search_deviceProfiles_group = value;
                RaisePropertyChanged("Search_deviceProfiles_group");
            }
        }
        public String Search_deviceProfiles_others
        {
            get { return _search_deviceProfiles_others; }
            set
            {
                _search_deviceProfiles_others = value;
                RaisePropertyChanged("Search_deviceProfiles_others");
            }
        }

        public ICommand ImportProfilesCommand { get; set; }
        public ICommand ClassManagementCommand { get; set; }
        public ICommand SearchClassProfilesCommand { get; set; }
        public ICommand SearchGroupProfilesCommand { get; set; }
        public ICommand SearchOthersProfilesCommand { get; set; }




        public AppPageViewModel()
        {
            ReloadDataProfiles();
            ReloadDataCardTypes();

            SearchOthersProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    SearchProfiles(p);
                });

            SearchClassProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDataProfiles((Search_profiles_class=="All"?"":Search_profiles_class), (Search_profiles_group=="All"?"": Search_profiles_group));
                });

            SearchGroupProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDataProfiles((Search_profiles_class=="All"?"":Search_profiles_class), (Search_profiles_group=="All"?"": Search_profiles_group));
                });

            ImportProfilesCommand = new RelayCommand<Profile>(
                (p) => true,
                (p) =>
                {
                    ReloadDataCardTypes();
                    IEnumerable<CardType> obsCollection = (IEnumerable<CardType>)Classes;
                    List<CardType> list = new List<CardType>(obsCollection);
                    ImportProfiles(list);
                    ReloadDataProfiles((Search_profiles_class == "All" ? "" : Search_profiles_class), (Search_profiles_group == "All" ? "" : Search_profiles_group));
                });

            ClassManagementCommand = new RelayCommand<CardType>(
                (p) => true,
                (p) =>
                {
                    ManageClass();
                    ReloadDataCardTypes();
                });
        }

        private void SearchProfiles(ItemCollection p)
        {
            Console.WriteLine("==================================");
            Console.WriteLine(Search_profiles_class);
            Console.WriteLine(Search_profiles_group);
            Console.WriteLine(Search_profiles_others);
            Console.WriteLine(Search_deviceProfiles_class);
            Console.WriteLine(Search_deviceProfiles_group);
            Console.WriteLine(Search_deviceProfiles_others);
            Console.WriteLine("==================================");

            p.Filter = (obj) => (
                (((Profile)obj).ADDRESS.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).AD_NO.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).EMAIL.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).PROFILE_NAME.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).PHONE.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).PIN_NO.ToLower().Contains(Search_profiles_others.ToString().ToLower()))
            );
        }

        public void ManageClass()
        {
            ClassManagement classManagement = new ClassManagement();
            classManagement.ShowDialog();
        }

        public void ImportProfiles(List<CardType> classes)
        {
            ImportWindow importWindow = new ImportWindow(classes);
            importWindow.ShowDialog();
        }

        public void ReloadDataCardTypes()
        {
            try
            {
                Classes.Clear();
                List<CardType> classesList = SqliteDataAccess.LoadAllCardType();
                Classes.Add(new CardType(0, "All"));
                foreach (CardType item in classesList)
                {
                    Classes.Add(item);
                }

            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        public void ReloadDataProfiles(string className = "", string subClass = "")
        {
            try
            {
                Profiles.Clear();
                List<Profile> profileList = SqliteDataAccess.LoadAllProfiles(className, subClass);
                foreach (Profile item in profileList)
                {
                    Profiles.Add(item);
                }

            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }
    }
}
