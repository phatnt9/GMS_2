using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GateAccessControl
{
    public class AddDeviceViewModels : ViewModelBase
    {
        public enum DeviceStatus
        {
            Pending,
            Connected,
            Disconnected
        }

        private bool? _dialogResult;
        private string _addDeviceStatus;
        private Device _addDevice;

        public bool? DialogResult
        {
            get => _dialogResult;
            set
            {
                _dialogResult = value;
                RaisePropertyChanged("DialogResult");
            }
        }

        public Device AddDevice
        {
            get => _addDevice;
            set
            {
                _addDevice = value;
                RaisePropertyChanged("AddDevice");
            }
        }

        public String AddDeviceStatus
        {
            get => _addDeviceStatus;
            set
            {
                _addDeviceStatus = value;
                RaisePropertyChanged("AddDeviceStatus");
            }
        }

        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ICommand AddDeviceCommand { get; set; }
        public ICommand CloseAddDeviceCommand { get; set; }

        public AddDeviceViewModels()
        {
            AddDevice = new Device();
            AddDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    if (ValidateIPv4(AddDevice.deviceIp) && !String.IsNullOrEmpty(AddDevice.deviceName))
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
                    AddDevice.deviceStatus = DeviceStatus.Pending.ToString();
                    InsertDeviceAsync();
                }
                );

            CloseAddDeviceCommand = new RelayCommand<Device>(
               (p) =>
               {
                   return true;
               },
               (p) =>
               {
                   CloseWindow();
               });
        }

        public async Task InsertDeviceAsync()
        {
            Task<bool> insertTask = SqliteDataAccess.InsertDeviceAsync(AddDevice);
            if (await insertTask)
            {
                AddDeviceStatus = "Succeed";
            }
            else
            {
                AddDeviceStatus = "Unsucceed";
            }
        }

        public void CloseWindow()
        {
            DialogResult = true;
        }

        public bool ValidateIPv4(string ipString)
        {
            IPAddress IP;
            bool flag = IPAddress.TryParse(ipString, out IP);
            if (flag)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}