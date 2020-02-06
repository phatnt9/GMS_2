using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GateAccessControl
{
    internal class ClassManagementViewModel : ViewModelBase
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
                    ReloadDataCardTypesAsync();
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
                    RemoveClassesAsync(p);
                    ReloadDataCardTypesAsync();
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

            ReloadDataCardTypesAsync();
        }

        public void SaveClasses(List<CardType> classes)
        {
            foreach (CardType cardType in classes)
            {
                if (cardType.classId == 0)
                {
                    SqliteDataAccess.InsertCardTypesAsync(cardType);
                }
                else
                {
                    SqliteDataAccess.UpdateCardTypeAsync(cardType);
                }
            }
        }

        public void RemoveClassesAsync(List<CardType> classes)
        {
            foreach (CardType cardType in classes)
            {
                Task<bool> deleteTask = SqliteDataAccess.DeleteCardTypeAsync(cardType);
            }
        }

        public async void ReloadDataCardTypesAsync()
        {
            Task<List<CardType>> loadTask = SqliteDataAccess.LoadCardTypesAsync();
            List<CardType> list = await loadTask;
            Classes = new ObservableCollection<CardType>(list);
        }

        public void CloseWindow()
        {
            DialogResult = true;
        }
    }
}