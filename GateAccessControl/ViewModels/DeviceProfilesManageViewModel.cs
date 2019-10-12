using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace GateAccessControl
{
    class DeviceProfilesManageViewModel : ViewModelBase
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool? _dialogResult;
        public bool? DialogResult
        {
            get
            {
                return _dialogResult;
            }
            set
            {
                _dialogResult = value;
                RaisePropertyChanged("DialogResult");
            }
        }

        private Device _device;
        public Device Device
        {
            get
            {
                return _device;
            }
            set
            {
                _device = value;
                RaisePropertyChanged("Device");
            }
        }

        private ObservableCollection<Profile> _profiles = new ObservableCollection<Profile>();
        private ObservableCollection<DeviceProfiles> _deviceProfiles = new ObservableCollection<DeviceProfiles>();
        public ObservableCollection<Profile> Profiles => _profiles;
        public ObservableCollection<DeviceProfiles> DeviceProfiles => _deviceProfiles;

        public ICommand CloseDeviceProfilesManagementCommand { get; set; }
        public ICommand SelectProfilesCommand { get; set; }
        public ICommand DeleteDeviceProfilesCommand { get; set; }


        public DeviceProfilesManageViewModel(Device device)
        {
            Device = device;
            ReloadDataProfiles();
            ReloadDataDeviceProfiles(device);

            DeleteDeviceProfilesCommand = new RelayCommand<List<DeviceProfiles>>(
                (p) =>
                {
                    if (p.Count > 0)
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
                    DeleteProfiles(p);
                    ReloadDataDeviceProfiles(Device);
                });

            SelectProfilesCommand = new RelayCommand<List<Profile>>(
                (p) =>
                {
                    if (p.Count > 0)
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
                    SelectProfiles(p);
                    ReloadDataDeviceProfiles(Device);
                });

            CloseDeviceProfilesManagementCommand = new RelayCommand<DeviceProfiles>(
                (p) =>
                {
                    return true;
                },
                (p) =>
                {
                    CloseWindow();
                });
        }

        public void DeleteProfiles(List<DeviceProfiles> profiles)
        {
            foreach (DeviceProfiles item in profiles)
            {
                SqliteDataAccess.DeleteDataDeviceProfiles("DT_DEVICE_PROFILES_" + Device.DEVICE_ID, item);
            }
        }

        public void SelectProfiles(List<Profile> profiles)
        {
            foreach (Profile item in profiles)
            {
                SqliteDataAccess.InsertDataDeviceProfiles("DT_DEVICE_PROFILES_" + Device.DEVICE_ID, new DeviceProfiles(item));
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
        public void CloseWindow()
        {
            DialogResult = true;
        }
    }
}
