using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace GateAccessControl
{
    internal class EditProfileViewModel : ViewModelBase
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

        private Profile _editProfile;

        public Profile EditProfile
        {
            get => _editProfile;
            set
            {
                _editProfile = value;
                RaisePropertyChanged("EditProfile");
            }
        }

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

        public ICommand SaveProfileCommand { get; set; }
        public ICommand CloseEditProfileCommand { get; set; }
        public ICommand ReplaceProfileImageCommand { get; set; }

        public EditProfileViewModel(Profile profile)
        {
            EditProfile = profile;
            ReloadDataCardTypesAsync();

            ReplaceProfileImageCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return (EditProfile != null) ? true : false;
                 },
                 (p) =>
                 {
                     ReplaceProfileImage(EditProfile.image, EditProfile);
                 });

            SaveProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     //Can Stop when sending Profiles
                     return (EditProfile != null) ? true : false;
                 },
                 (p) =>
                 {
                     SaveProfileAsync(EditProfile);
                 });

            CloseEditProfileCommand = new RelayCommand<Device>(
              (p) =>
              {
                  return true;
              },
              (p) =>
              {
                  CloseWindow();
              });
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
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string importFilePath = openFileDialog1.FileName;
                string fileName = openFileDialog1.SafeFileName;
                if (String.IsNullOrEmpty(origin))
                {
                    p.image = fileName;
                }
                else
                {
                    p.image = origin;
                }
                File.Copy(importFilePath,
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + p.image, true);
                p.image = p.image;
            }
        }

        public void CloseWindow()
        {
            DialogResult = true;
        }

        private async Task<bool> SaveProfileAsync(Profile p)
        {
            if (p.profileStatus.Equals(GlobalConstant.ProfileStatus.Active.ToString()))
            {
                if (p.check_date_to_lock && DateTime.Now.CompareTo(p.date_to_lock) >= 0)
                {
                    //Today is later then date to lock
                    p.check_date_to_lock = false;
                    p.profileStatus = GlobalConstant.ProfileStatus.Suspended.ToString();
                    p.date_modified = DateTime.Now;
                }
            }
            else
            {
                p.check_date_to_lock = false;
                p.date_modified = DateTime.Now;
            }
            Task<bool> updateTask = SqliteDataAccess.UpdateProfileAsync(p);
            if (await updateTask)
            {
                System.Windows.Forms.MessageBox.Show("Profile saved!");
                UpdateProfileToAllDeviceAsync(p);
                return true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Field with (*) is mandatory!");
                return false;
            }
        }

        private async Task<bool> UpdateProfileToAllDeviceAsync(Profile p)
        {
            if (p != null)
            {
                int count = 0;
                List<int> listDeviceId = new List<int>();

                if (!String.IsNullOrEmpty(p.list_device_id))
                {
                    string[] listVar = p.list_device_id.Split(',');
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
                        List<DeviceProfile> getCloneDeviceProfile = await SqliteDataAccess.LoadDeviceProfilesAsync(id, "", "", p.pinno);

                        foreach (DeviceProfile DP in getCloneDeviceProfile)
                        {
                            DP.CloneDataFromProfile(p);
                            if (p.profileStatus.Equals(GlobalConstant.ProfileStatus.Suspended.ToString()))
                            {
                                if (!DP.client_status.Equals(GlobalConstant.ClientStatus.Deleted.ToString()))
                                {
                                    DP.client_status = GlobalConstant.ClientStatus.Delete.ToString();
                                }
                            }
                            else
                            {
                                DP.client_status = GlobalConstant.ClientStatus.Unknow.ToString();
                                if (DP.server_status.Equals(GlobalConstant.ServerStatus.None.ToString()))
                                {
                                    DP.server_status = GlobalConstant.ServerStatus.Update.ToString();
                                }
                            }
                            Task<bool> updateTask = SqliteDataAccess.UpdateDeviceProfileAsync(id, DP);
                            if (await updateTask)
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
            else
            {
                return false;
            }
        }

        public async void ReloadDataCardTypesAsync()
        {
            Task<List<CardType>> loadTask = SqliteDataAccess.LoadCardTypesAsync();
            List<CardType> classesList = await loadTask;
            Classes = new ObservableCollection<CardType>(classesList);
        }
    }
}