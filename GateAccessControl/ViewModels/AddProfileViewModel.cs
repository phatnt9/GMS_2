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
    class AddProfileViewModel : ViewModelBase
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


        private Profile _addProfile;
        public Profile AddProfile
        {
            get
            {
                return _addProfile;
            }
            set
            {
                _addProfile = value;
                RaisePropertyChanged("AddProfile");
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

        public ICommand InsertProfileCommand { get; set; }
        public ICommand CloseAddProfileCommand { get; set; }
        public ICommand ReplaceProfileImageCommand { get; set; }

        public AddProfileViewModel()
        {
            AddProfile = new Profile();
            ReloadDataCardTypes();

            ReplaceProfileImageCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return (AddProfile != null) ? true : false;
                 },
                 (p) =>
                 {
                     ReplaceProfileImage(AddProfile.IMAGE, AddProfile);
                 });

            InsertProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     //Can Stop when sending Profiles
                     return (AddProfile != null) ? true : false;
                 },
                 (p) =>
                 {
                     InsertProfile(AddProfile);
                 });

            CloseAddProfileCommand = new RelayCommand<Device>(
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

        private void InsertProfile(Profile addProfile)
        {
            addProfile.DATE_CREATED = DateTime.Now;
            addProfile.DATE_MODIFIED = DateTime.Now;
            if (SqliteDataAccess.InsertProfile(addProfile))
            {
                //Success
                System.Windows.Forms.MessageBox.Show("Profile added!");
                AddProfile = new Profile();
            }
            else
            {
                //Unsuccess
                System.Windows.Forms.MessageBox.Show("Field with (*) is mandatory!");
            }
        }

        public void CloseWindow()
        {
            DialogResult = true;
        }
        public void ReloadDataCardTypes()
        {
            try
            {
                Classes = new ObservableCollection<CardType>(SqliteDataAccess.LoadCardTypes());
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }
    }
}
