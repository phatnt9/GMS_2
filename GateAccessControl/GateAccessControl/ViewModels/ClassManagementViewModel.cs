using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace GateAccessControl
{
    class ClassManagementViewModel : ViewModelBase
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

        private ObservableCollection<CardType> _classes = new ObservableCollection<CardType>();
        public ObservableCollection<CardType> Classes => _classes;

        public ICommand RemoveClassesCommand { get; set; }
        public ICommand SaveClassesCommand { get; set; }
        public ICommand CloseClassManagementCommand { get; set; }
        

        public ClassManagementViewModel()
        {

            SaveClassesCommand = new RelayCommand<ObservableCollection<CardType>>(
                (p) =>
                {
                    return true;
                },
                (p) =>
                {
                    List<CardType> list = p.ToList();
                    SaveClasses(list);
                    ReloadDataCardTypes();
                });

            RemoveClassesCommand = new RelayCommand<List<CardType>>(
                (p) =>
                {
                    if (p != null)
                    {
                        return (p.Count > 0) ? true : false;
                    }
                    else
                    {
                        return false;
                    }
                },
                (p) =>
                {
                    RemoveClasses(p);
                    ReloadDataCardTypes();
                });

            CloseClassManagementCommand = new RelayCommand<CardType>(
                (p) =>
                {
                    return true;
                },
                (p) =>
                {
                    CloseWindow();
                });


            ReloadDataCardTypes();
        }

        public void SaveClasses(List<CardType> classes)
        {
            foreach (CardType cardType in classes)
            {
                if (cardType.CLASS_ID == 0)
                {
                    SqliteDataAccess.InsertDataClass(cardType);
                }
                else
                {
                    SqliteDataAccess.UpdateDataClass(cardType);
                }
            }
        }

        public void RemoveClasses(List<CardType> classes)
        {
            foreach (CardType cardType in classes)
            {
                SqliteDataAccess.DeleteDataCardType(cardType);
            }
        }

        public void ReloadDataCardTypes()
        {
            try
            {
                Classes.Clear();
                List<CardType> classesList = SqliteDataAccess.LoadAllCardType();
                foreach (CardType item in classesList)
                {
                    Classes.Add(item);
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
