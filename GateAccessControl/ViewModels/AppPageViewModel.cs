using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Controls;
using GateAccessControl.Views;
using System.ComponentModel;
using WebSocketSharp;

namespace GateAccessControl
{
    public class AppPageViewModel : ViewModelBase
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Device _selectedDevice;

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

        private int _syncProgressValue;

        private BackgroundWorker SyncWorker;

        public int SyncProgressValue
        {
            get { return _syncProgressValue; }
            set
            {
                _syncProgressValue = value;
                RaisePropertyChanged("SyncProgressValue");
            }
        }


        public Device SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                _selectedDevice = value;
                RaisePropertyChanged("SelectedDevice");
            }
        }

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
        public ICommand EditDeviceCommand { get; set; }
        public ICommand RemoveDeviceCommand { get; set; }


        public ICommand SelectDeviceCommand { get; set; }
        public ICommand DeviceProfilesManageCommand { get; set; }




        public ICommand ConnectDeviceCommand { get; set; }
        public ICommand DisconnectDeviceCommand { get; set; }


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

        public string CheckDeviceConnected(Device device)
        {
            try
            {
                WebSocket vl = device.DeviceItem.webSocket;
                if (vl != null)
                {
                    if (vl.IsAlive)
                    {
                        device.DeviceItem.WebSocketStatus = "Connected";
                        return "Connected";
                    }
                    else
                    {
                        device.DeviceItem.WebSocketStatus = "Connecting";
                        return "Connecting";
                    }
                }
                else
                {
                    device.DeviceItem.WebSocketStatus = "Disconnected";
                    return "Disconnected";
                }
            }
            catch
            {
                return "Pending";
            }
        }


        public AppPageViewModel()
        {
            ReloadDataDevices();
            ReloadDataProfiles();
            ReloadDataCardTypes();
            SyncProgressValue = 0;


            StopSyncCommand = new RelayCommand<Device>(
                 (p) =>
                 {
                     if (p.DeviceItem.IsSendingProfiles)
                     { return true; }
                     else
                     { return false; }
                 },
                 (p) =>
                 {
                     AddDevice();
                     ReloadDataDevices();
                 });


            SyncCommand = new RelayCommand<List<DeviceProfiles>>(
                 (p) =>
                 {
                     if (SelectedDevice == null)
                     {
                         return false;
                     }
                     if (p == null || SelectedDevice.DeviceItem.IsSendingProfiles) 
                     {
                         return false;
                     }
                     if (!CheckDeviceConnected(SelectedDevice).Equals("Connected"))
                     {
                         return false;
                     }
                     List<DeviceProfiles> CanSyncDeviceProfiles = p.FindAll((u) =>
                     {
                         if (
                         ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                         (u.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString())) ||

                         ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                         (u.SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString())) ||

                         ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                         (u.SERVER_STATUS == GlobalConstant.ServerStatus.Update.ToString())) ||

                         ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
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

                     });
                     if (p.Count > 0 && CanSyncDeviceProfiles.Count == p.Count)
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
                    SyncDeviceProfiles(p);
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
                    if(RemoveDevice(p))
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
                    SqliteDataAccess.CreateDeviceProfilesTable("DT_DEVICE_PROFILES_"+p.DEVICE_ID);
                    ReloadDataDeviceProfiles(p);
                });

            DeviceProfilesManageCommand = new RelayCommand<Device>(
                (p) =>
                {
                    if (p != null)
                        return true;
                    else
                        return false;
                },
                (p) =>
                {
                    ManageDeviceProfiles(SelectedDevice);
                    ReloadDataDeviceProfiles(SelectedDevice);
                    ReloadDataProfiles((Search_profiles_class == "All" ? "" : Search_profiles_class), (Search_profiles_group == "All" ? "" : Search_profiles_group));
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
                    if (p.DeviceItem.IsSendingProfiles)
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
                    ReloadDataProfiles((Search_profiles_class=="All"?"":Search_profiles_class), (Search_profiles_group=="All"?"": Search_profiles_group));
                });

            SearchClassDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDataDeviceProfiles(SelectedDevice, (Search_deviceProfiles_class=="All"?"": Search_deviceProfiles_class), (Search_deviceProfiles_group=="All"?"": Search_deviceProfiles_group));
                });

            SearchGroupProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDataProfiles((Search_profiles_class == "All" ? "" : Search_profiles_class), (Search_profiles_group == "All" ? "" : Search_profiles_group));
                });

            SearchGroupDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDataDeviceProfiles(SelectedDevice, (Search_deviceProfiles_class == "All"?"": Search_deviceProfiles_class), (Search_deviceProfiles_group == "All"?"": Search_deviceProfiles_group));
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

        private void SyncDeviceProfiles(List<DeviceProfiles> profiles)
        {
            SyncWorker = new BackgroundWorker();
            SyncWorker.WorkerSupportsCancellation = true;
            SyncWorker.WorkerReportsProgress = true;
            SyncWorker.DoWork += SyncWorker_DoWork;
            SyncWorker.RunWorkerCompleted += SyncWorker_RunWorkerCompleted;
            SyncWorker.ProgressChanged += SyncWorker_ProgressChanged;
            SyncWorker.RunWorkerAsync(argument: profiles);
        }

        private void SyncWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SyncProgressValue = e.ProgressPercentage;
        }

        private void SyncWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // handle the error
                //PgbStatus = AppStatus.Error;
            }
            else if (e.Cancelled)
            {
                // handle cancellation
                //PgbStatus = AppStatus.Cancelled;
            }
            else
            {
                //PgbStatus = AppStatus.Completed;
            }
            SyncProgressValue = 0;
        }

        private void SyncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool remainProfiles = true;
            List<DeviceProfiles> profiles = (List<DeviceProfiles>)e.Argument;
            for (int i = 0; i < profiles.Count; i++)
            {
                if (i == (profiles.Count - 1))
                {
                    remainProfiles = false;
                }
                DeviceProfiles deviceProfileToSend = profiles[i];

                if ((deviceProfileToSend.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (deviceProfileToSend.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString()))
                {
                    deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                    deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();


                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_ADD;
                    List<DeviceProfiles> sendList = new List<DeviceProfiles>();
                    sendList.Add(deviceProfileToSend);
                    if (SelectedDevice.DeviceItem.SendDeviceProfile(SelectedDevice.DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + SelectedDevice.DEVICE_ID, deviceProfileToSend);
                        continue;
                    }
                    else
                    {
                        deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
                    }
                    if (SyncWorker.CancellationPending)
                    {
                        break;
                    }
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);
                }

                if ((deviceProfileToSend.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (deviceProfileToSend.SERVER_STATUS == GlobalConstant.ServerStatus.Update.ToString()))
                {
                    deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                    deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();


                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_UPDATE;
                    List<DeviceProfiles> sendList = new List<DeviceProfiles>();
                    sendList.Add(deviceProfileToSend);
                    if (SelectedDevice.DeviceItem.SendDeviceProfile(SelectedDevice.DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + SelectedDevice.DEVICE_ID, deviceProfileToSend);
                        continue;
                    }
                    else
                    {
                        deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Update.ToString();
                    }
                    if (SyncWorker.CancellationPending)
                    {
                        break;
                    }
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);
                }

                if ((deviceProfileToSend.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (deviceProfileToSend.SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString()))
                {
                    deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                    deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();


                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    List<DeviceProfiles> sendList = new List<DeviceProfiles>();
                    sendList.Add(deviceProfileToSend);
                    if (SelectedDevice.DeviceItem.SendDeviceProfile(SelectedDevice.DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + SelectedDevice.DEVICE_ID, deviceProfileToSend);
                        continue;
                    }
                    else
                    {
                        deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Remove.ToString();
                    }
                    if (SyncWorker.CancellationPending)
                    {
                        break;
                    }
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);
                }

                if ((deviceProfileToSend.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (deviceProfileToSend.SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString()))
                {
                    deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                    deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();


                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    List<DeviceProfiles> sendList = new List<DeviceProfiles>();
                    sendList.Add(deviceProfileToSend);
                    if (SelectedDevice.DeviceItem.SendDeviceProfile(SelectedDevice.DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + SelectedDevice.DEVICE_ID, deviceProfileToSend);
                        continue;
                    }
                    else
                    {
                        deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Remove.ToString();
                    }
                    if (SyncWorker.CancellationPending)
                    {
                        break;
                    }
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);
                }

                if ((deviceProfileToSend.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (deviceProfileToSend.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString()))
                {
                    deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                    deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();


                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_ADD;
                    List<DeviceProfiles> sendList = new List<DeviceProfiles>();
                    sendList.Add(deviceProfileToSend);
                    if (SelectedDevice.DeviceItem.SendDeviceProfile(SelectedDevice.DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + SelectedDevice.DEVICE_ID, deviceProfileToSend);
                        continue;
                    }
                    else
                    {
                        deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
                    }
                    if (SyncWorker.CancellationPending)
                    {
                        break;
                    }
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);
                }
            }
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
            if(SqliteDataAccess.DeleteDataDevice(p))
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
                List<DeviceProfiles> deviceProfileList = SqliteDataAccess.LoadAllDeviceProfiles(device, className, subClass);
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

        private bool CanEditOrRemoveDevice(Device p)
        {
            if (p != null)
            {
                if (p.DeviceItem.webSocket == null) // Disconnected
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
    }
}
