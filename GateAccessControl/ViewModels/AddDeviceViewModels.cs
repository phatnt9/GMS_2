using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
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

        public String AddDeviceStatus
        {
            get
            {
                return _addDeviceStatus;
            }
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
            AddDeviceCommand = new RelayCommand<Device>(
                (p) => 
                {
                    if (ValidateIPv4(p.DEVICE_IP) && !String.IsNullOrEmpty(p.DEVICE_NAME.ToString()))
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
                    p.DEVICE_STATUS = DeviceStatus.Pending.ToString();
                    if(SqliteDataAccess.InsertDataDevice(p))
                    {
                        AddDeviceStatus = "Succeed";
                    }
                    else
                    {
                        AddDeviceStatus = "Error";
                    }
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
