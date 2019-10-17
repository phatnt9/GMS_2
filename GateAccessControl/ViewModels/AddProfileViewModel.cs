using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
        public ObservableCollection<CardType> Classes => _classes;

        public ICommand InsertProfileCommand { get; set; }
        public ICommand CloseAddProfileCommand { get; set; }

        public AddProfileViewModel()
        {
            AddProfile = new Profile();
            ReloadDataCardTypes();


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

        private void InsertProfile(Profile addProfile)
        {
            addProfile.DATE_CREATED = DateTime.Now;
            addProfile.DATE_MODIFIED = DateTime.Now;
            if (SqliteDataAccess.InsertDataProfile(addProfile))
            {
                //Success
                MessageBox.Show("Profile added!");
                AddProfile = new Profile();
            }
            else
            {
                //Unsuccess
                MessageBox.Show("Field with (*) is mandatory!");
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
