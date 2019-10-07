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
        private ObservableCollection<Device> _devices = new ObservableCollection<Device>();
        private ObservableCollection<DeviceProfiles> _deviceProfiles = new ObservableCollection<DeviceProfiles>();
        private ObservableCollection<TimeRecord> _timeChecks = new ObservableCollection<TimeRecord>();
        private ObservableCollection<CardType> _classes = new ObservableCollection<CardType>();

        public ObservableCollection<Profile> Profiles => _profiles;
        public ObservableCollection<Device> Devices => _devices;
        public ObservableCollection<DeviceProfiles> DeviceProfiles => _deviceProfiles;
        public ObservableCollection<TimeRecord> TimeChecks => _timeChecks;
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


        public ICommand AddDeviceCommand { get; set; }
        public ICommand SelectDeviceCommand { get; set; }
        public ICommand ConnectDeviceCommand { get; set; }
        public ICommand DisconnectDeviceCommand { get; set; }
        public ICommand ImportProfilesCommand { get; set; }
        public ICommand ManageClassCommand { get; set; }
        public ICommand SearchClassProfilesCommand { get; set; }
        public ICommand SearchGroupProfilesCommand { get; set; }
        public ICommand SearchOthersProfilesCommand { get; set; }

        
        

        public AppPageViewModel()
        {
            ReloadDataDevices();
            ReloadDataProfiles();
            ReloadDataCardTypes();

            SelectDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return true;
                },
                (p) =>
                {
                    SqliteDataAccess.CreateDeviceProfilesTable("DT_DEVICE_PROFILES_"+p.DEVICE_ID);
                    ReloadDataDeviceProfiles(p);
                });

            ConnectDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return CheckIfDeviceCanConnect(p);
                },
                (p) =>
                {
                    ConnectDevice(p);
                });

            DisconnectDeviceCommand = new RelayCommand<Device>(
                (p) => 
                {
                    return !CheckIfDeviceCanConnect(p);
                },
                (p) =>
                {
                    DisconnectDevice(p);
                });

            AddDeviceCommand = new RelayCommand<Device>(
                (p) => true,
                (p) =>
                {
                    AddDevice(p);
                    ReloadDataDevices();
                });

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

            ManageClassCommand = new RelayCommand<CardType>(
                (p) => true,
                (p) =>
                {
                    ManageClass();
                    ReloadDataCardTypes();
                });
        }

        private void ReloadDataDeviceProfiles(Device p)
        {
            try
            {
                _deviceProfiles.Clear();
                List<DeviceProfiles> deviceProfileList = SqliteDataAccess.LoadAllDeviceProfiles(p);
                foreach (DeviceProfiles item in deviceProfileList)
                {
                    _deviceProfiles.Add(item);
                }

            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private void DisconnectDevice(Device p)
        {
            p.DeviceItem.Dispose();
        }

        private void ConnectDevice(Device p)
        {
            p.DeviceItem.Start("ws://" + p.DEVICE_IP + ":9090");
        }

        private void AddDevice(Device p)
        {
            AddDeviceWindow addDeviceWindow = new AddDeviceWindow();
            addDeviceWindow.ShowDialog();
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
                _classes.Clear();
                List<CardType> classesList = SqliteDataAccess.LoadAllCardType();
                Classes.Add(new CardType(0, "All"));
                foreach (CardType item in classesList)
                {
                    _classes.Add(item);
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        public void ReloadDataDevices(Device removedDevice = null)
        {
            try
            {
                List<Device> deviceList = SqliteDataAccess.LoadAllDevices();
                foreach (Device item in deviceList)
                {
                    if (!CheckExistDeviceRF(Devices, item))
                    {
                        Devices.Add(item);
                    }
                }
                if (removedDevice != null)
                {
                    Devices.Remove(removedDevice);
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        public bool CheckExistDeviceRF(ObservableCollection<Device> list, Device deviceRF)
        {
            foreach (Device item in list)
            {
                if ((item.DEVICE_IP == deviceRF.DEVICE_IP))
                {
                    //item.DEVICE_ID = deviceRF.DEVICE_ID;
                    //item.DEVICE_NAME = deviceRF.DEVICE_NAME;
                    //item.DEVICE_STATUS = deviceRF.DEVICE_STATUS;
                    return true;
                }
            }
            return false;
        }

        public void ReloadDataProfiles(string className = "", string subClass = "")
        {
            try
            {
                _profiles.Clear();
                List<Profile> profileList = SqliteDataAccess.LoadAllProfiles(className, subClass);
                foreach (Profile item in profileList)
                {
                    _profiles.Add(item);
                }

            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        public bool CheckIfDeviceCanConnect(Device p)
        {
            if (p != null)
            {
                if (p.DeviceItem.webSocket != null)
                {
                    if (p.DeviceItem.webSocket.IsAlive)
                    {
                        //p.DEVICE_STATUS = "Connected";
                    }
                    else
                    {
                        //p.DEVICE_STATUS = "Connecting";
                    }
                    return false;
                }
                else
                {
                    //p.DEVICE_STATUS = "Pending";
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
