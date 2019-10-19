using GateAccessControl.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using WebSocketSharp;

namespace GateAccessControl
{
    public class AppPageViewModel : ViewModelBase
    {
        public ICommand AddDeviceCommand { get; set; }
        public ICommand EditDeviceCommand { get; set; }
        public ICommand RemoveDeviceCommand { get; set; }
        public ICommand SelectDeviceCommand { get; set; }
        public ICommand ConnectDeviceCommand { get; set; }
        public ICommand DisconnectDeviceCommand { get; set; }



        public ICommand AddProfileCommand { get; set; }
        public ICommand EditProfileCommand { get; set; }
        public ICommand RemoveProfileCommand { get; set; }
        public ICommand SelectProfileCommand { get; set; }



        public ICommand DeviceProfilesManageCommand { get; set; }
        
        public ICommand ImportProfilesCommand { get; set; }
        public ICommand ManageClassCommand { get; set; }

        public ICommand SearchClassProfilesCommand { get; set; }
        public ICommand SearchGroupProfilesCommand { get; set; }
        public ICommand SearchOthersProfilesCommand { get; set; }

        public ICommand SearchClassDeviceProfilesCommand { get; set; }
        public ICommand SearchGroupDeviceProfilesCommand { get; set; }
        public ICommand SearchOthersDeviceProfilesCommand { get; set; }

        public ICommand SyncCommand { get; set; }
        public ICommand StopSyncCommand { get; set; }
        public ICommand ReplaceProfileImageCommand { get; set; }

        public ICommand SetTimeDeviceProfileCommnad { get; set; }

        public string CheckDeviceStatus(Device device)
        {
            if (device.DeviceItem.webSocket != null)
            {
                if (device.DeviceItem.webSocket.IsAlive)
                {
                    return "Connected";
                }
                else
                {
                    return "Connecting";
                }
            }
            else
            {
                return "Disconnected";
            }
        }

        public AppPageViewModel()
        {
            ReloadDataDevices();
            ReloadDataProfiles();
            ReloadDataCardTypes();
            SyncProgressValue = 0;
            CreateCheckSuspendProfilesTimer();
            CreateRequestTimeChecksTimer();

            SetTimeDeviceProfileCommnad = new RelayCommand<string>(
                 (p) =>
                 {
                     if (p != null)
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
                     //AddProfile(p);
                     string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                     string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                     ReloadDataProfiles(classSearch, groupSearch);
                 });

            AddProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return true;
                 },
                 (p) =>
                 {
                     AddProfile(p);
                     string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                     string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                     ReloadDataProfiles(classSearch, groupSearch);
                 });

            EditProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     //Cannot when any device is syncing
                     //Console.WriteLine("_numberOfSyncingDevices: "+ CheckNoDeviceIsSyncing());
                     return (p != null && CheckNoDeviceIsSyncing() == 0) ? true : false;
                 },
                 (p) =>
                 {
                     EditProfile(p);
                     string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                     string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                     ReloadDataProfiles(classSearch, groupSearch);
                     if(SelectedDevice != null)
                     {
                         ReloadDataDeviceProfiles(SelectedDevice);
                     }
                 });

            RemoveProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     //Cannot when any device is syncing
                     return (p != null && CheckNoDeviceIsSyncing() == 0) ? true : false;
                 },
                 (p) =>
                 {
                     RemoveProfile(p);
                     string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                     string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                     ReloadDataProfiles(classSearch, groupSearch);
                 });

            SelectProfileCommand = new RelayCommand<List<TimeRecord>>(
                 (p) =>
                 {
                     return true;
                 },
                 (p) =>
                 {
                     ReloadDataTimeCheck(p);
                 });

            ReplaceProfileImageCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     //Can Stop when sending Profiles
                     return (p != null && SelectedDevice != null) ? true : false;
                 },
                 (p) =>
                 {
                     string origin = p.IMAGE;
                     p.IMAGE = "default.png";
                     ReplaceProfileImage(origin,p);
                     p.IMAGE = origin;
                     ReloadDataDeviceProfiles(SelectedDevice);

                 });

            StopSyncCommand = new RelayCommand<Device>(
                 (p) =>
                 {
                     //Can Stop when sending Profiles
                     return (p!= null && p.DeviceItem.IsSendingProfiles) ? true : false;
                 },
                 (p) =>
                 {
                     p.SyncWorker.CancelAsync();
                 });

            SyncCommand = new RelayCommand<List<DeviceProfiles>>(
                 (p) =>
                 {
                     /* 
                      * User must select a Device and some Profiles to be able to Sync Profiles.
                      * Device must be connected before Syncing.
                      * If that Device is in process of Sending something, then you cannot Sync too.
                      * Get list profiles that can be synced depend on its status and server Status
                      */
                     if (SelectedDevice != null && p != null)
                     {
                         if (SelectedDevice.DeviceItem.IsSendingProfiles || !CheckDeviceStatus(SelectedDevice).Equals("Connected"))
                         {
                             return false;
                         } //right
                         //if (SelectedDevice.DeviceItem.IsSendingProfiles)
                         //{
                         //    return false;
                         //} //wrong
                         return (GetCanSyncDeviceProfiles(p).Count > 0) ? true : false;
                     }
                     else
                     {
                         return false;
                     }
                 },
                (p) =>
                {
                    //Sync it
                    SelectedDevice.SyncDeviceProfiles(GetCanSyncDeviceProfiles(p));
                });

            AddDeviceCommand = new RelayCommand<Device>(
                 (p) => true,
                 (p) =>
                 {
                     AddDevice();
                     ReloadDataDevices();
                 });

            EditDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return CanEditOrRemoveDevice(p);
                },
                (p) =>
                {
                    EditDevice(p);
                    ReloadDataDevices();
                });

            RemoveDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return CanEditOrRemoveDevice(p);
                },
                (p) =>
                {
                    if (RemoveDevice(p))
                    {
                        ReloadDataDevices(p);
                    }
                });

            SelectDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return true;
                },
                (p) =>
                {
                    SqliteDataAccess.CreateDeviceProfilesTable("DT_DEVICE_PROFILES_" + p.DEVICE_ID);
                    ReloadDataDeviceProfiles(p);
                });

            DeviceProfilesManageCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return (p != null && !p.DeviceItem.IsSendingProfiles) ? true : false;
                },
                (p) =>
                {
                    ManageDeviceProfiles(p);
                    ReloadDataDeviceProfiles(p);
                    string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                    string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                    ReloadDataProfiles(classSearch, groupSearch);
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
                    if (p == null || p.DeviceItem.IsSendingProfiles)
                    {
                        return false;
                    }
                    else
                    {
                        return !CheckIfDeviceCanConnect(p);
                    }
                },
                (p) =>
                {
                    DisconnectDevice(p);
                });

            SearchOthersProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    SearchProfiles(p);
                });

            SearchOthersDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    SearchDeviceProfiles(p);
                });

            SearchClassProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                    string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                    ReloadDataProfiles(classSearch, groupSearch);
                });

            SearchGroupProfilesCommand = new RelayCommand<ItemCollection>(
                 (p) => true,
                 (p) =>
                 {
                     string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                     string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                     ReloadDataProfiles(classSearch, groupSearch);
                 });

            SearchClassDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDataDeviceProfiles(SelectedDevice, (Search_deviceProfiles_class == "All" ? "" : Search_deviceProfiles_class), (Search_deviceProfiles_group == "All" ? "" : Search_deviceProfiles_group));
                });

            

            SearchGroupDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDataDeviceProfiles(SelectedDevice, (Search_deviceProfiles_class == "All" ? "" : Search_deviceProfiles_class), (Search_deviceProfiles_group == "All" ? "" : Search_deviceProfiles_group));
                });

            ImportProfilesCommand = new RelayCommand<Profile>(
                (p) => true,
                (p) =>
                {
                    ReloadDataCardTypes();
                    IEnumerable<CardType> obsCollection = (IEnumerable<CardType>)Classes;
                    List<CardType> list = new List<CardType>(obsCollection);
                    ImportProfiles(list);
                    string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                    string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                    ReloadDataProfiles(classSearch, groupSearch);
                });

            ManageClassCommand = new RelayCommand<CardType>(
                (p) => true,
                (p) =>
                {
                    ManageClass();
                    ReloadDataCardTypes();
                });
        }

        private void CreateRequestTimeChecksTimer()
        {
            System.Timers.Timer RequestTimeChecksTimer = new System.Timers.Timer(); //One second, (use less to add precision, use more to consume less processor time
            RequestTimeChecksTimer.Interval = Properties.Settings.Default.RequestTimeCheckInterval;
            RequestTimeChecksTimer.Elapsed += RequestTimeChecksTimer_Elapsed;
            RequestTimeChecksTimer.AutoReset = true;
            RequestTimeChecksTimer.Start();
        }

        private void RequestTimeChecksTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("RequestTimeChecksTimer_Elapsed");
            if (CheckNoDeviceIsSyncing() == 0)
            {
                Task.Run(() =>
                {
                    try
                    {
                        foreach (Device device in Devices)
                        {
                            device.DeviceItem.RequestPersonListImmediately();
                        }
                    }
                    catch (Exception ex)
                    {
                        logFile.Error(ex.Message);
                    }
                });
            }
        }
        
        private void CreateCheckSuspendProfilesTimer()
        {
            System.Timers.Timer SuspendStudentCheckTimer = new System.Timers.Timer(30000); //One second, (use less to add precision, use more to consume less processor time
            lastHour = DateTime.Now.Hour;
            lastSec = DateTime.Now.Second;
            SuspendStudentCheckTimer.Elapsed += SuspendStudentCheckTimer_Elapsed;
            SuspendStudentCheckTimer.Start();
        }
        private void SuspendStudentCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(CheckNoDeviceIsSyncing() == 0)
            {
                if (lastHour < DateTime.Now.Hour || (lastHour == 23 && DateTime.Now.Hour == 0))
                {
                    lastHour = DateTime.Now.Hour;
                    Console.WriteLine("Catch " + DateTime.Now.ToLongTimeString() + "s.");
                    CheckSuspendAllProfile();
                }
            }
        }

        public void CheckSuspendAllProfile()
        {
            try
            {
                //Get all Profile
                List<Profile> profiles = SqliteDataAccess.LoadAllProfiles();

                //Check status --> check date to Suspend --> Suspend(active)
                foreach (Profile profile in profiles)
                {
                    if (profile.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Active.ToString()) && 
                        profile.CHECK_DATE_TO_LOCK == true)
                    {
                        if (DateTime.Now > profile.DATE_TO_LOCK)
                        {
                            profile.CHECK_DATE_TO_LOCK = false;
                            profile.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                            profile.LOCK_DATE = DateTime.Now;
                            profile.DATE_MODIFIED = DateTime.Now;
                            if (SqliteDataAccess.UpdateDataProfile(profile))
                            {
                                //MessageBox.Show("Profile saved!");
                                UpdateProfileToAllDevice(profile);
                            }
                            else
                            {
                                //MessageBox.Show("Field with (*) is mandatory!");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
            finally
            {
                string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                ReloadDataProfiles(classSearch, groupSearch);
            }
        }

        private bool UpdateProfileToAllDevice(Profile p)
        {
            int count = 0;
            List<int> listDeviceId = new List<int>();

            if (!String.IsNullOrEmpty(p.LIST_DEVICE_ID))
            {
                string[] listVar = p.LIST_DEVICE_ID.Split(',');
                foreach (string var in listVar)
                {
                    int temp;
                    Int32.TryParse(var, out temp);
                    if (temp != 0)
                    {
                        listDeviceId.Add(temp);
                    }
                }
                foreach (int id in listDeviceId)
                {
                    List<DeviceProfiles> getCloneDeviceProfile = SqliteDataAccess.LoadAllDeviceProfiles(id, "", "", p.PIN_NO);

                    foreach (DeviceProfiles DP in getCloneDeviceProfile)
                    {
                        DP.CloneDataFromProfile(p);

                        if (p.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Suspended.ToString()))
                        {
                            if (!DP.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Deleted.ToString()))
                            {
                                DP.CLIENT_STATUS = GlobalConstant.ClientStatus.Delete.ToString();
                            }
                        }
                        else
                        {
                            DP.CLIENT_STATUS = GlobalConstant.ClientStatus.Unknow.ToString();
                            if (DP.SERVER_STATUS.Equals(GlobalConstant.ServerStatus.None.ToString()))
                            {
                                DP.SERVER_STATUS = GlobalConstant.ServerStatus.Update.ToString();
                            }
                        }

                        if (SqliteDataAccess.UpdateDataDeviceProfiles(id, DP))
                        {
                            count++;
                        }
                    }

                }
                if (count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void RemoveProfile(Profile p)
        {
            if (String.IsNullOrEmpty(p.LIST_DEVICE_ID))
            {
                SqliteDataAccess.DeleteDataProfile(p);
            }
        }

        private void AddProfile(Profile p)
        {
            AddProfileWindow addProfileWindow = new AddProfileWindow();
            addProfileWindow.ShowDialog();
        }

        private void EditProfile(Profile p)
        {
            EditProfileWindow editProfileWindow = new EditProfileWindow(p);
            editProfileWindow.ShowDialog();
        }

        private void ReplaceProfileImage(string origin, Profile p)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Title = "Browse JPEG Image",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "JPEG",
                Filter = "All JPEG Files (*.jpg)|*.jpg",
                FilterIndex = 1,
                RestoreDirectory = true
                //ReadOnlyChecked = true,
                //ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //img_profile.Source = null;
                //string oldFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + tb_image.Text;
                //File.Delete(oldFilePath);
                string importFilePath = openFileDialog1.FileName;
                File.Copy(importFilePath,
               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + origin, true);

                p.IMAGE = origin;
                SaveProfileUpdateImage(p);


            }
        }
        
        public void SaveProfileUpdateImage(Profile p)
        {

        }

        private void ManageDeviceProfiles(Device p)
        {
            DeviceProfilesManagement deviceProfilesWindow = new DeviceProfilesManagement(p);
            deviceProfilesWindow.ShowDialog();
        }

        private void AddDevice()
        {
            AddDeviceWindow addDeviceWindow = new AddDeviceWindow();
            addDeviceWindow.ShowDialog();
        }

        private void EditDevice(Device p)
        {
            EditDeviceWindow editDeviceWindow = new EditDeviceWindow(p);
            editDeviceWindow.ShowDialog();
        }

        private bool RemoveDevice(Device p)
        {
            //Remove Device in database
            if (SqliteDataAccess.DeleteDataDevice(p))
            {
                //Succeed
                Console.WriteLine("Succeed");
                return true;
            }
            else
            {
                //Unsucceed
                Console.WriteLine("Unsucceed");
                return false;
            }
        }

        private void ReloadDataDeviceProfiles(Device p)
        {
            try
            {
                _deviceProfiles.Clear();
                List<DeviceProfiles> deviceProfileList = SqliteDataAccess.LoadAllDeviceProfiles(p.DEVICE_ID);
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
            p.DeviceItem.checkAlive.Start();
            p.DeviceItem.Start("ws://" + p.DEVICE_IP + ":9090");
        }

        private void SearchProfiles(ItemCollection p)
        {
            p.Filter = (obj) => (
                (((Profile)obj).ADDRESS.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).AD_NO.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).EMAIL.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).PROFILE_NAME.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).PHONE.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).PIN_NO.ToLower().Contains(Search_profiles_others.ToString().ToLower()))
            );
        }

        private void SearchDeviceProfiles(ItemCollection p)
        {
            p.Filter = (obj) => (
                (((Profile)obj).ADDRESS.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((Profile)obj).AD_NO.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((Profile)obj).EMAIL.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((Profile)obj).PROFILE_NAME.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((Profile)obj).PHONE.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((Profile)obj).PIN_NO.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower()))
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
                    Device device = CheckExistDeviceRF(Devices, item);
                    if (device == null)
                    {
                        //Add Device
                        Devices.Add(item);
                    }
                    else
                    {
                        //Update Device
                        device.DEVICE_IP = item.DEVICE_IP;
                        device.DEVICE_NAME = item.DEVICE_NAME;
                        device.DEVICE_NOTE = item.DEVICE_NOTE;
                    }
                }
                //Remove Device
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

        public Device CheckExistDeviceRF(ObservableCollection<Device> list, Device deviceRF)
        {
            foreach (Device item in list)
            {
                if ((item.DEVICE_ID == deviceRF.DEVICE_ID))
                {
                    //item.DEVICE_IP = deviceRF.DEVICE_IP;
                    //item.DEVICE_NAME = deviceRF.DEVICE_NAME;
                    //item.DEVICE_STATUS = deviceRF.DEVICE_STATUS;
                    return item;
                }
            }
            return null;
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

        public void ReloadDataDeviceProfiles(Device device, string className = "", string subClass = "")
        {
            try
            {
                _deviceProfiles.Clear();
                List<DeviceProfiles> deviceProfileList = SqliteDataAccess.LoadAllDeviceProfiles(device.DEVICE_ID, className, subClass);
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

        public void ReloadDataTimeCheck(List<TimeRecord> timeCheckList)
        {
            try
            {
                _timeChecks.Clear();
                foreach (TimeRecord item in timeCheckList)
                {
                    _timeChecks.Add(item);
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private bool CanEditOrRemoveDevice(Device p)
        {
            if (p != null && p.DeviceItem.WebSocketStatus != null)
            {
                if (p.DeviceItem.WebSocketStatus.Equals("Disconnected") && !p.DeviceItem.IsSendingProfiles)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
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

        public int CheckNoDeviceIsSyncing()
        {
            int _noDeviceSyncing = 0;
            foreach (Device item in Devices)
            {
                if(item.DeviceItem.IsSendingProfiles)
                {
                    _noDeviceSyncing++;
                }
            }
            return _noDeviceSyncing;
        }



        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _search_profiles_class;
        private string _search_profiles_group;
        private string _search_profiles_others;

        private string _search_deviceProfiles_class;
        private string _search_deviceProfiles_group;
        private string _search_deviceProfiles_others;

        private Device _selectedDevice;
        private int _syncProgressValue;

        private int lastHour = DateTime.Now.Hour;
        private int lastSec = DateTime.Now.Second;

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

        
        public int SyncProgressValue
        {
            get => _syncProgressValue;
            set
            {
                _syncProgressValue = value;
                RaisePropertyChanged("SyncProgressValue");
            }
        }

        public Device SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                RaisePropertyChanged("SelectedDevice");
            }
        }

        public String Search_profiles_class
        {
            get => _search_profiles_class;
            set
            {
                _search_profiles_class = value;
                RaisePropertyChanged("Search_profiles_class");
            }
        }

        public String Search_profiles_group
        {
            get => _search_profiles_group;
            set
            {
                _search_profiles_group = value;
                RaisePropertyChanged("Search_profiles_group");
            }
        }

        public String Search_profiles_others
        {
            get => _search_profiles_others;
            set
            {
                _search_profiles_others = value;
                RaisePropertyChanged("Search_profiles_others");
            }
        }

        public String Search_deviceProfiles_class
        {
            get => _search_deviceProfiles_class;
            set
            {
                _search_deviceProfiles_class = value;
                RaisePropertyChanged("Search_deviceProfiles_class");
            }
        }

        public String Search_deviceProfiles_group
        {
            get => _search_deviceProfiles_group;
            set
            {
                _search_deviceProfiles_group = value;
                RaisePropertyChanged("Search_deviceProfiles_group");
            }
        }

        public String Search_deviceProfiles_others
        {
            get => _search_deviceProfiles_others;
            set
            {
                _search_deviceProfiles_others = value;
                RaisePropertyChanged("Search_deviceProfiles_others");
            }
        }

        public List<DeviceProfiles> GetCanSyncDeviceProfiles(List<DeviceProfiles> p)
        {
            List<DeviceProfiles> CanSyncDeviceProfiles = p.FindAll((u) =>
            {
                switch(u.CLIENT_STATUS)
                {
                    case "Unknow":
                    {
                        goto case "Delete";
                    }
                    case "Delete":
                    {
                        if (
                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString())) ||

                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString())) ||

                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Update.ToString())) ||

                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString())) ||

                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString()))
                           )
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    case "Deleted":
                    {
                        return false;
                    }
                    default:
                    {
                        return false;
                    }
                }
            });
            return CanSyncDeviceProfiles;
        }

    }
}