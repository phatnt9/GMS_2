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
        public ICommand ActiveDeviceProfilesCommand { get; set; }
        public ICommand DeactiveDeviceProfilesCommand { get; set; }


        public DeviceProfilesManageViewModel(Device device)
        {
            Device = device;
            ReloadDataProfiles();
            ReloadDataDeviceProfiles(device);

            DeactiveDeviceProfilesCommand = new RelayCommand<List<DeviceProfiles>>(
                (p) =>
                {
                    if (p == null)
                    {
                        return false;
                    }
                    List<DeviceProfiles> CanDeactiveDeviceProfiles = p.FindAll((u) =>
                    {
                        if (
                        ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (u.SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString())) ||

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
                    if (p != null && p.Count > 0 && CanDeactiveDeviceProfiles.Count == p.Count)
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
                    DeactiveDeviceProfiles(p);
                    ReloadDataDeviceProfiles(Device);
                });

            ActiveDeviceProfilesCommand = new RelayCommand<List<DeviceProfiles>>(
                (p) =>
                {
                    if(p == null)
                    {
                        return false;
                    }
                    List<DeviceProfiles> CanActiveDeviceProfiles = p.FindAll((u) =>
                    {
                        if (
                        ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (u.SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString())) ||

                        ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (u.SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString()))
                        )
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    });
                    if (p != null && p.Count > 0 && CanActiveDeviceProfiles.Count == p.Count)
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
                    ActiveDeviceProfiles(p);
                    ReloadDataDeviceProfiles(Device);
                });

            DeleteDeviceProfilesCommand = new RelayCommand<List<DeviceProfiles>>(
                (p) =>
                {
                    if (p == null)
                    {
                        return false;
                    }
                    List< DeviceProfiles> CanDeleteDeviceProfiles = p.FindAll((u) => 
                    {
                        if (
                        ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (u.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString())) ||

                        ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (u.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString())) ||

                        ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (u.SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString()))
                        )
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    });
                if (p != null && p.Count > 0 && CanDeleteDeviceProfiles.Count == p.Count)
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
                    DeleteDeviceProfiles(p);
                    ReloadDataDeviceProfiles(Device);
                });

            SelectProfilesCommand = new RelayCommand<List<Profile>>(
                (p) =>
                {
                    List<Profile> CanInserDeviceProfiles = p.FindAll((u) =>
                    {
                        if (
                        (u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString())
                        )
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    });
                    if (p.Count > 0 && CanInserDeviceProfiles.Count == p.Count)
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
                    InserDeviceProfiles(p);
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

        private void ActiveDeviceProfiles(List<DeviceProfiles> profiles)
        {
            foreach (DeviceProfiles item in profiles)
            {
                if ((item.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (item.SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString()))
                {
                    item.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                    item.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();

                    if (SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + Device.DEVICE_ID, item))
                    {
                        continue;
                    }
                    else
                    {
                        item.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        item.SERVER_STATUS = GlobalConstant.ServerStatus.Remove.ToString();
                    }
                }

                if ((item.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (item.SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString()))
                {
                    item.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                    item.SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();

                    if (SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + Device.DEVICE_ID, item))
                    {
                        continue;
                    }
                    else
                    {
                        item.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                        item.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                    }
                }
            }
        }

        private void DeactiveDeviceProfiles(List<DeviceProfiles> profiles)
        {
            foreach (DeviceProfiles item in profiles)
            {
                if ((item.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (item.SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString()))
                {
                    item.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                    item.SERVER_STATUS = GlobalConstant.ServerStatus.Remove.ToString();

                    if (SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + Device.DEVICE_ID, item))
                    {
                        continue;
                    }
                    else
                    {
                        item.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        item.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                    }
                }

                if ((item.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (item.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString()))
                {
                    item.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                    item.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();

                    if (SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + Device.DEVICE_ID, item))
                    {
                        continue;
                    }
                    else
                    {
                        item.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                        item.SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
                    }
                }
            }
        }

        public void DeleteDeviceProfiles(List<DeviceProfiles> profiles)
        {
            foreach (DeviceProfiles item in profiles)
            {
                if(SqliteDataAccess.DeleteDataDeviceProfiles("DT_DEVICE_PROFILES_" + Device.DEVICE_ID, item))
                {
                    List<Profile> listProfiles = Profiles.Where(x => x.PIN_NO.Equals(item.PIN_NO)).Cast<Profile>().ToList();
                    foreach (Profile pf in listProfiles)
                    {
                        pf.RemoveDeviceId(Device.DEVICE_ID);
                        SqliteDataAccess.UpdateDataProfile(pf);
                    }
                }
            }
        }

        public void InserDeviceProfiles(List<Profile> profiles)
        {
            foreach (Profile item in profiles)
            {
                if (SqliteDataAccess.InsertDataDeviceProfiles("DT_DEVICE_PROFILES_" + Device.DEVICE_ID, new DeviceProfiles(item)))
                {
                    item.AddDeviceId(Device.DEVICE_ID);
                    SqliteDataAccess.UpdateDataProfile(item);
                }

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
