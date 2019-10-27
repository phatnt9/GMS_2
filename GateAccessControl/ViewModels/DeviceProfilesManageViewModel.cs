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
        private ObservableCollection<CardType> _classes = new ObservableCollection<CardType>();

        public ObservableCollection<CardType> Classes => _classes;
        public ObservableCollection<Profile> Profiles => _profiles;
        public ObservableCollection<DeviceProfiles> DeviceProfiles => _deviceProfiles;

        public ICommand CloseDeviceProfilesManagementCommand { get; set; }
        public ICommand SelectProfilesCommand { get; set; }
        public ICommand DeleteDeviceProfilesCommand { get; set; }
        public ICommand ActiveDeviceProfilesCommand { get; set; }
        public ICommand DeactiveDeviceProfilesCommand { get; set; }
        public ICommand SearchClassProfilesCommand { get; set; }
        public ICommand SearchGroupProfilesCommand { get; set; }
        public ICommand SearchOthersProfilesCommand { get; set; }

        public ICommand SearchClassDeviceProfilesCommand { get; set; }
        public ICommand SearchGroupDeviceProfilesCommand { get; set; }
        public ICommand SearchOthersDeviceProfilesCommand { get; set; }

        public DeviceProfilesManageViewModel(Device device)
        {
            Device = device;
            ReloadDataCardTypes();
            ReloadProfiles();
            ReloadDeviceProfiles(device);

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
                    ReloadProfiles();
                });

            SearchGroupProfilesCommand = new RelayCommand<ItemCollection>(
                 (p) => true,
                 (p) =>
                 {
                     ReloadProfiles();
                 });

            SearchClassDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDeviceProfiles(Device);
                });

            SearchGroupDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDeviceProfiles(Device);
                });

            DeactiveDeviceProfilesCommand = new RelayCommand<List<DeviceProfiles>>(
                (p) =>
                {
                    if (p != null)
                    {
                        List<DeviceProfiles> CanDeactiveDeviceProfiles = p.FindAll((u) =>
                        {
                            if (
                            ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                            (u.CLIENT_STATUS == GlobalConstant.ClientStatus.Unknow.ToString()) &&
                            (u.SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString())) ||

                            ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                            (u.CLIENT_STATUS == GlobalConstant.ClientStatus.Unknow.ToString()) &&
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
                        if (p.Count > 0 && CanDeactiveDeviceProfiles.Count == p.Count)
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
                    
                },
                (p) =>
                {
                    DeactiveDeviceProfiles(p);
                    //ReloadDataDeviceProfiles(Device);
                });

            ActiveDeviceProfilesCommand = new RelayCommand<List<DeviceProfiles>>(
                (p) =>
                {
                    if(p != null)
                    {
                        List<DeviceProfiles> CanActiveDeviceProfiles = p.FindAll((u) =>
                        {
                            if (
                            ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                            (u.CLIENT_STATUS == GlobalConstant.ClientStatus.Unknow.ToString()) &&
                            (u.SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString())) ||

                            ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                            (u.CLIENT_STATUS == GlobalConstant.ClientStatus.Unknow.ToString()) &&
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
                        if (p.Count > 0 && CanActiveDeviceProfiles.Count == p.Count)
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
                },
                (p) =>
                {
                    ActiveDeviceProfiles(p);
                    //ReloadDataDeviceProfiles(Device);
                });

            DeleteDeviceProfilesCommand = new RelayCommand<List<DeviceProfiles>>(
                (p) =>
                {
                    if (p != null)
                    {
                        List<DeviceProfiles> CanDeleteDeviceProfiles = p.FindAll((u) =>
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
                        if (p.Count > 0 && CanDeleteDeviceProfiles.Count == p.Count)
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
                },
                (p) =>
                {
                    DeleteDeviceProfiles(p);
                    ReloadDeviceProfiles(Device);
                });

            SelectProfilesCommand = new RelayCommand<List<Profile>>(
                (p) =>
                {
                    if (p != null)
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
                    }
                    else
                    {
                        return false;
                    }
                },
                (p) =>
                {
                    InserDeviceProfiles(p);
                    ReloadDeviceProfiles(Device);
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

                    if (SqliteDataAccess.UpdateDataDeviceProfiles(Device.DEVICE_ID, item))
                    {
                        continue;
                    }
                    else
                    {
                        item.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        item.SERVER_STATUS = GlobalConstant.ServerStatus.Remove.ToString();
                    }
                    continue;
                }

                if ((item.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (item.SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString()))
                {
                    item.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                    item.SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();

                    if (SqliteDataAccess.UpdateDataDeviceProfiles(Device.DEVICE_ID, item))
                    {
                        continue;
                    }
                    else
                    {
                        item.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                        item.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                    }
                    continue;
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

                    if (SqliteDataAccess.UpdateDataDeviceProfiles(Device.DEVICE_ID, item))
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

                    if (SqliteDataAccess.UpdateDataDeviceProfiles(Device.DEVICE_ID, item))
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
                if(SqliteDataAccess.DeleteDataDeviceProfiles(Device.DEVICE_ID, item))
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
                if (SqliteDataAccess.InsertDataDeviceProfiles(Device.DEVICE_ID, new DeviceProfiles(item)))
                {
                    item.AddDeviceId(Device.DEVICE_ID);
                    SqliteDataAccess.UpdateDataProfile(item);
                }

            }
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

        public void CloseWindow()
        {
            DialogResult = true;
        }

        public void ReloadProfiles()
        {
            if (Search_profiles_class == null)
            {
                Search_profiles_class = "";
            }
            if (Search_profiles_group == null)
            {
                Search_profiles_group = "";
            }
            string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
            string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
            ReloadDataProfiles(classSearch, groupSearch);
        }

        public void ReloadDeviceProfiles(Device p)
        {
            if (p!= null)
            {
                if (Search_deviceProfiles_class == null)
                {
                    Search_deviceProfiles_class = "";
                }
                if (Search_deviceProfiles_group == null)
                {
                    Search_deviceProfiles_group = "";
                }
                string classSearch = Search_deviceProfiles_class == "All" ? "" : Search_deviceProfiles_class;
                string groupSearch = Search_deviceProfiles_group == "All" ? "" : Search_deviceProfiles_group;
                ReloadDataDeviceProfiles(p, classSearch, groupSearch);
            }
        }

        private void SearchProfiles(ItemCollection p)
        {
            if (p != null)
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
        }

        private void SearchDeviceProfiles(ItemCollection p)
        {
            if (p != null)
            {
                p.Filter = (obj) => (
                (((DeviceProfiles)obj).ADDRESS.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfiles)obj).AD_NO.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfiles)obj).EMAIL.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfiles)obj).PROFILE_NAME.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfiles)obj).PHONE.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfiles)obj).PIN_NO.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower()))
            );
            }
        }


        private string _search_profiles_class;
        private string _search_profiles_group;
        private string _search_profiles_others;
        private string _search_deviceProfiles_class;
        private string _search_deviceProfiles_group;
        private string _search_deviceProfiles_others;

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
    }
}
