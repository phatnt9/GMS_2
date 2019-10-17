using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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

        public ICommand SaveProfileCommand { get; set; }
        public ICommand CloseEditProfileCommand { get; set; }

        public EditProfileViewModel(Profile profile)
        {
            EditProfile = profile;

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
                    p.LOCK_DATE = DateTime.Now;
                    p.DATE_MODIFIED = DateTime.Now;
                }
            }
            else
            {
                p.CHECK_DATE_TO_LOCK = false;
                p.LOCK_DATE = DateTime.Now;
                p.DATE_MODIFIED = DateTime.Now;
            }

            if (UpdateProfileToAllDevice(p))
            {
                if(SqliteDataAccess.UpdateDataProfile(p))
                {
                    MessageBox.Show("Profile saved!");
                    return true;
                }
                else
                {
                    MessageBox.Show("Field with (*) is mandatory!");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Profile saved!");
                return true;
            }
        }

        private bool UpdateProfileToAllDevice(Profile p)
        {
            int count = 0;
            List<int> listDeviceId = new List<int>();
            DeviceProfiles deviceProfiles = new DeviceProfiles(p);

            if (deviceProfiles.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString())
            {
                deviceProfiles.SERVER_STATUS = GlobalConstant.ServerStatus.Remove.ToString();
            }

            if (deviceProfiles.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString())
            {
                deviceProfiles.SERVER_STATUS = GlobalConstant.ServerStatus.Update.ToString();
            }

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
                    if (SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + id, deviceProfiles))
                    {
                        count++;
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
    }
}
