﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace GateAccessControl
{
    internal class DeviceProfilesManageViewModel : ViewModelBase
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool? _dialogResult;

        public bool? DialogResult
        {
            get => _dialogResult;
            set
            {
                _dialogResult = value;
                RaisePropertyChanged("DialogResult");
            }
        }

        private Device _device;

        public Device Device
        {
            get => _device;
            set
            {
                _device = value;
                RaisePropertyChanged("Device");
            }
        }

        private ObservableCollection<Profile> _profiles = new ObservableCollection<Profile>();
        private ObservableCollection<DeviceProfile> _deviceProfiles = new ObservableCollection<DeviceProfile>();
        private ObservableCollection<CardType> _classes = new ObservableCollection<CardType>();

        public ObservableCollection<CardType> Classes
        {
            get => _classes;
            set
            {
                _classes = value;
                RaisePropertyChanged("Classes");
            }
        }

        public ObservableCollection<Profile> Profiles
        {
            get => _profiles;
            set
            {
                _profiles = value;
                RaisePropertyChanged("Profiles");
            }
        }

        public ObservableCollection<DeviceProfile> DeviceProfiles
        {
            get => _deviceProfiles;
            set
            {
                _deviceProfiles = value;
                RaisePropertyChanged("DeviceProfiles");
            }
        }

        public ICommand CloseDeviceProfilesManagementCommand { get; set; }
        public ICommand SelectProfilesCommand { get; set; }
        public ICommand StopSelectProfilesCommand { get; set; }
        public ICommand DeleteDeviceProfilesCommand { get; set; }
        public ICommand StopDeleteProfilesCommand { get; set; }
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
            ReloadCardTypes();
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

            DeactiveDeviceProfilesCommand = new RelayCommand<List<DeviceProfile>>(
                (p) =>
                {
                    if (p != null && IsSelectingProfiles == false && IsDeletingProfiles == false)
                    {
                        List<DeviceProfile> CanDeactiveDeviceProfiles = p.FindAll((u) =>
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
                        return (p.Count > 0 && CanDeactiveDeviceProfiles.Count == p.Count);
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

            ActiveDeviceProfilesCommand = new RelayCommand<List<DeviceProfile>>(
                (p) =>
                {
                    if (p != null && IsSelectingProfiles == false && IsDeletingProfiles == false)
                    {
                        List<DeviceProfile> CanActiveDeviceProfiles = p.FindAll((u) =>
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
                        return (p.Count > 0 && CanActiveDeviceProfiles.Count == p.Count);
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

            DeleteDeviceProfilesCommand = new RelayCommand<List<DeviceProfile>>(
                (p) =>
                {
                    if (p != null && IsSelectingProfiles == false && IsDeletingProfiles == false)
                    {
                        List<DeviceProfile> CanDeleteDeviceProfiles = p.FindAll((u) =>
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
                        return (p.Count > 0 && CanDeleteDeviceProfiles.Count == p.Count);
                    }
                    else
                    {
                        return false;
                    }
                },
                (p) =>
                {
                    DeleteDeviceProfiles(p);
                });

            SelectProfilesCommand = new RelayCommand<List<Profile>>(
                (p) =>
                {
                    if (p != null && IsSelectingProfiles == false && IsDeletingProfiles == false)
                    {
                        List<Profile> CanInserDeviceProfiles = p.FindAll((u) =>
                        {
                            return (u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString());
                        });
                        return (p.Count > 0 && CanInserDeviceProfiles.Count == p.Count);
                    }
                    else
                    {
                        return false;
                    }
                },
                (p) =>
                {
                    InserDeviceProfiles(p);
                });

            StopSelectProfilesCommand = new RelayCommand<List<Profile>>(
                (p) =>
                {
                    if (IsSelectingProfiles == true)
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
                    SelectWorker.CancelAsync();
                });

            StopDeleteProfilesCommand = new RelayCommand<List<Profile>>(
                (p) =>
                {
                    if (IsDeletingProfiles == true)
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
                    if (DeleteWorker != null && DeleteWorker.IsBusy)
                    {
                        DeleteWorker.CancelAsync();
                    }
                    if (DeactiveWorker != null && DeactiveWorker.IsBusy)
                    {
                        DeactiveWorker.CancelAsync();
                    }
                    if (ActiveWorker != null && ActiveWorker.IsBusy)
                    {
                        ActiveWorker.CancelAsync();
                    }
                });

            CloseDeviceProfilesManagementCommand = new RelayCommand<DeviceProfile>(
                (p) =>
                {
                    return true;
                },
                (p) =>
                {
                    CloseWindow();
                });
        }

        private void ActiveDeviceProfiles(List<DeviceProfile> deviceProfiles)
        {
            DeleteProgressValue = 0;
            ActiveWorker = new BackgroundWorker();
            ActiveWorker.WorkerSupportsCancellation = true;
            ActiveWorker.WorkerReportsProgress = true;
            ActiveWorker.DoWork += ActiveWorker_DoWork;
            ActiveWorker.RunWorkerCompleted += ActiveWorker_RunWorkerCompleted;
            ActiveWorker.ProgressChanged += ActiveWorker_ProgressChanged;
            ActiveWorker.RunWorkerAsync(argument: deviceProfiles);
            
        }

        private void ActiveWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DeleteProgressValue = e.ProgressPercentage;
        }

        private void ActiveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // handle the error
            }
            else if (e.Cancelled)
            {
                // handle cancellation
            }
            else
            {
                // handle complete
            }
            DeleteProgressValue = 0;
            IsDeletingProfiles = false;
            ReloadDeviceProfiles(Device);
        }

        private void ActiveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsDeletingProfiles = true;
            List<DeviceProfile> deviceProfiles = e.Argument as List<DeviceProfile>;
            for (int i = 0; i < deviceProfiles.Count; i++)
            {
                if ((deviceProfiles[i].PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (deviceProfiles[i].SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString()))
                {
                    deviceProfiles[i].PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                    deviceProfiles[i].SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();

                    if (SqliteDataAccess.UpdateDeviceProfile(Device.DEVICE_ID, deviceProfiles[i]))
                    {
                        //continue;
                    }
                    else
                    {
                        deviceProfiles[i].PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        deviceProfiles[i].SERVER_STATUS = GlobalConstant.ServerStatus.Remove.ToString();
                    }
                }

                if ((deviceProfiles[i].PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (deviceProfiles[i].SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString()))
                {
                    deviceProfiles[i].PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                    deviceProfiles[i].SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();

                    if (SqliteDataAccess.UpdateDeviceProfile(Device.DEVICE_ID, deviceProfiles[i]))
                    {
                        //continue;
                    }
                    else
                    {
                        deviceProfiles[i].PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                        deviceProfiles[i].SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                    }
                }

                (sender as BackgroundWorker).ReportProgress((i * 100) / deviceProfiles.Count);
                if (ActiveWorker.CancellationPending)
                {
                    break;
                }
            }
        }

        private void DeactiveDeviceProfiles(List<DeviceProfile> deviceProfiles)
        {
            DeleteProgressValue = 0;
            DeactiveWorker = new BackgroundWorker();
            DeactiveWorker.WorkerSupportsCancellation = true;
            DeactiveWorker.WorkerReportsProgress = true;
            DeactiveWorker.DoWork += DeactiveWorker_DoWork;
            DeactiveWorker.RunWorkerCompleted += DeactiveWorker_RunWorkerCompleted;
            DeactiveWorker.ProgressChanged += DeactiveWorker_ProgressChanged;
            DeactiveWorker.RunWorkerAsync(argument: deviceProfiles);

        }

        private void DeactiveWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DeleteProgressValue = e.ProgressPercentage;
        }

        private void DeactiveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // handle the error
            }
            else if (e.Cancelled)
            {
                // handle cancellation
            }
            else
            {
                // handle complete
            }
            DeleteProgressValue = 0;
            IsDeletingProfiles = false;
            ReloadDeviceProfiles(Device);
        }

        private void DeactiveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsDeletingProfiles = true;
            List<DeviceProfile> deviceProfiles = e.Argument as List<DeviceProfile>;
            for (int i = 0; i < deviceProfiles.Count; i++)
            {
                if ((deviceProfiles[i].PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (deviceProfiles[i].SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString()))
                {
                    deviceProfiles[i].PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                    deviceProfiles[i].SERVER_STATUS = GlobalConstant.ServerStatus.Remove.ToString();

                    if (SqliteDataAccess.UpdateDeviceProfile(Device.DEVICE_ID, deviceProfiles[i]))
                    {
                        //continue;
                    }
                    else
                    {
                        deviceProfiles[i].PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        deviceProfiles[i].SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                    }
                }

                if ((deviceProfiles[i].PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (deviceProfiles[i].SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString()))
                {
                    deviceProfiles[i].PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                    deviceProfiles[i].SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();

                    if (SqliteDataAccess.UpdateDeviceProfile(Device.DEVICE_ID, deviceProfiles[i]))
                    {
                        //continue;
                    }
                    else
                    {
                        deviceProfiles[i].PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                        deviceProfiles[i].SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
                    }
                }

                (sender as BackgroundWorker).ReportProgress((i * 100) / deviceProfiles.Count);
                if (DeactiveWorker.CancellationPending)
                {
                    break;
                }
            }
        }

        public void DeleteDeviceProfiles(List<DeviceProfile> deviceProfiles)
        {
            DeleteProgressValue = 0;
            DeleteWorker = new BackgroundWorker();
            DeleteWorker.WorkerSupportsCancellation = true;
            DeleteWorker.WorkerReportsProgress = true;
            DeleteWorker.DoWork += DeleteWorker_DoWork;
            DeleteWorker.RunWorkerCompleted += DeleteWorker_RunWorkerCompleted;
            DeleteWorker.ProgressChanged += DeleteWorker_ProgressChanged;
            DeleteWorker.RunWorkerAsync(argument: deviceProfiles);
        }

        private void DeleteWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DeleteProgressValue = e.ProgressPercentage;
        }

        private void DeleteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // handle the error
            }
            else if (e.Cancelled)
            {
                // handle cancellation
            }
            else
            {
                // handle complete
            }
            DeleteProgressValue = 0;
            IsDeletingProfiles = false;
            ReloadDeviceProfiles(Device);
        }

        private void DeleteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsDeletingProfiles = true;
            List<DeviceProfile> deviceProfiles = e.Argument as List<DeviceProfile>;
            for (int i = 0; i < deviceProfiles.Count; i++)
            {
                if (SqliteDataAccess.DeleteDeviceProfile(Device.DEVICE_ID, deviceProfiles[i]))
                {
                    List<Profile> listProfiles = Profiles.Where(x => x.PIN_NO.Equals(deviceProfiles[i].PIN_NO)).Cast<Profile>().ToList();
                    foreach (Profile pf in listProfiles)
                    {
                        pf.RemoveDeviceId(Device.DEVICE_ID);
                        SqliteDataAccess.UpdateProfile(pf);
                    }
                }
                
                (sender as BackgroundWorker).ReportProgress((i * 100) / deviceProfiles.Count);
                if (DeleteWorker.CancellationPending)
                {
                    break;
                }
            }
        }

        public void InserDeviceProfiles(List<Profile> profiles)
        {
            SelectProgressValue = 0;
            SelectWorker = new BackgroundWorker();
            SelectWorker.WorkerSupportsCancellation = true;
            SelectWorker.WorkerReportsProgress = true;
            SelectWorker.DoWork += SelectWorker_DoWork;
            SelectWorker.RunWorkerCompleted += SelectWorker_RunWorkerCompleted;
            SelectWorker.ProgressChanged += SelectWorker_ProgressChanged;
            SelectWorker.RunWorkerAsync(argument: profiles);
        }

        private void SelectWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SelectProgressValue = e.ProgressPercentage;
        }

        private void SelectWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // handle the error
            }
            else if (e.Cancelled)
            {
                // handle cancellation
            }
            else
            {
                // handle complete
            }
            SelectProgressValue = 0;
            IsSelectingProfiles = false;
            ReloadDeviceProfiles(Device);
        }

        private void SelectWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsSelectingProfiles = true;
            List<Profile> profiles = e.Argument as List<Profile>;


            for (int i = 0; i < profiles.Count; i++)
            {
                if (SqliteDataAccess.InsertDeviceProfile(Device.DEVICE_ID,new DeviceProfile(profiles[i])))
                {
                    profiles[i].AddDeviceId(Device.DEVICE_ID);
                    SqliteDataAccess.UpdateProfile(profiles[i]);
                }
                (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);
                if (SelectWorker.CancellationPending)
                {
                    break;
                }
            }
        }

        public void ReloadCardTypes()
        {
            try
            {
                List<CardType> classesList = SqliteDataAccess.LoadCardTypes();
                classesList.Insert(0, new CardType(-1, "All"));
                Classes = new ObservableCollection<CardType>(classesList);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        public bool ReloadProfiles()
        {
            if (Search_profiles_class == null)
            {
                Search_profiles_class = "All";
            }
            if (Search_profiles_group == null)
            {
                Search_profiles_group = "All";
            }
            string type = Search_profiles_class == "All" ? "" : Search_profiles_class;
            string group = Search_profiles_group == "All" ? "" : Search_profiles_group;
            try
            {
                Profiles = new ObservableCollection<Profile>(SqliteDataAccess.LoadProfiles(type, group, ""));
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public bool ReloadDeviceProfiles(Device d)
        {
            if (Search_deviceProfiles_class == null)
            {
                Search_deviceProfiles_class = "All";
            }
            if (Search_deviceProfiles_group == null)
            {
                Search_deviceProfiles_group = "All";
            }
            string type = Search_deviceProfiles_class == "All" ? "" : Search_deviceProfiles_class;
            string group = Search_deviceProfiles_group == "All" ? "" : Search_deviceProfiles_group;
            try
            {
                DeviceProfiles = new ObservableCollection<DeviceProfile>(SqliteDataAccess.LoadDeviceProfiles(d.DEVICE_ID, type, group, ""));
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
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
                (((DeviceProfile)obj).ADDRESS.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfile)obj).AD_NO.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfile)obj).EMAIL.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfile)obj).PROFILE_NAME.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfile)obj).PHONE.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfile)obj).PIN_NO.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower()))
            );
            }
        }

        public void CloseWindow()
        {
            DialogResult = true;
        }

        private int _selectProgressValue;
        private int _deleteProgressValue;
        private bool _isSelectingProfiles;
        private bool _isDeletingProfiles;
        public BackgroundWorker SelectWorker;
        public BackgroundWorker DeleteWorker;
        public BackgroundWorker DeactiveWorker;
        public BackgroundWorker ActiveWorker;

        private string _search_profiles_class;
        private string _search_profiles_group;
        private string _search_profiles_others;
        private string _search_deviceProfiles_class;
        private string _search_deviceProfiles_group;
        private string _search_deviceProfiles_others;

        public bool IsDeletingProfiles
        {
            get => _isDeletingProfiles;
            set
            {
                _isDeletingProfiles = value;
                RaisePropertyChanged("IsDeletingProfiles");
            }
        }

        public bool IsSelectingProfiles
        {
            get => _isSelectingProfiles;
            set
            {
                _isSelectingProfiles = value;
                RaisePropertyChanged("IsSelectingProfiles");
            }
        }

        public int DeleteProgressValue
        {
            get => _deleteProgressValue;
            set
            {
                _deleteProgressValue = value;
                RaisePropertyChanged("DeleteProgressValue");
            }
        }

        public int SelectProgressValue
        {
            get => _selectProgressValue;
            set
            {
                _selectProgressValue = value;
                RaisePropertyChanged("SelectProgressValue");
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
    }
}