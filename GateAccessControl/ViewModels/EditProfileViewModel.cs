using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace GateAccessControl
{
    class EditProfileViewModel : ViewModelBase
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


        private Profile _editProfile;
        public Profile EditProfile
        {
            get
            {
                return _editProfile;
            }
            set
            {
                _editProfile = value;
                RaisePropertyChanged("EditProfile");
            }
        }

        private ObservableCollection<CardType> _classes = new ObservableCollection<CardType>();
        public ObservableCollection<CardType> Classes => _classes;

        public ICommand SaveProfileCommand { get; set; }
        public ICommand CloseEditProfileCommand { get; set; }
        public ICommand ReplaceProfileImageCommand { get; set; }

        public EditProfileViewModel(Profile profile)
        {
            EditProfile = profile;
            ReloadDataCardTypes();

            ReplaceProfileImageCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return (EditProfile != null) ? true : false;
                 },
                 (p) =>
                 {
                     ReplaceProfileImage(EditProfile.IMAGE, EditProfile);
                 });

            SaveProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     //Can Stop when sending Profiles
                     return (EditProfile != null) ? true : false;
                 },
                 (p) =>
                 {
                     SaveProfile(EditProfile);
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
                    p.IMAGE = fileName;
                }
                else
                {
                    p.IMAGE = origin;
                }
                File.Copy(importFilePath,
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + p.IMAGE, true);
                p.IMAGE = p.IMAGE;


            }
        }

        public void CloseWindow()
        {
            DialogResult = true;
        }
        private bool SaveProfile(Profile p)
        {
            if(p.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Active.ToString()))
            {
                if (p.CHECK_DATE_TO_LOCK && DateTime.Now.CompareTo(p.DATE_TO_LOCK) >= 0)
                {
                    //Today is later then date to lock
                    p.CHECK_DATE_TO_LOCK = false;
                    p.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                    p.DATE_MODIFIED = DateTime.Now;
                }
            }
            else
            {
                p.CHECK_DATE_TO_LOCK = false;
                p.DATE_MODIFIED = DateTime.Now;
            }

            if (SqliteDataAccess.UpdateDataProfile(p))
            {
                System.Windows.Forms.MessageBox.Show("Profile saved!");
                UpdateProfileToAllDevice(p);
                return true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Field with (*) is mandatory!");
                return false;
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
                    List<DeviceProfiles> getCloneDeviceProfile = SqliteDataAccess.LoadAllDeviceProfiles(id,"","",p.PIN_NO);

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
        public void ReloadDataCardTypes()
        {
            try
            {
                _classes.Clear();
                List<CardType> classesList = SqliteDataAccess.LoadAllCardType();
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
    }
}
