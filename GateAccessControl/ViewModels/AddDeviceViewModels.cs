﻿using System;
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
        private Device _addDevice;
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

        public Device AddDevice
        {
            get
            {
                return _addDevice;
            }
            set
            {
                _addDevice = value;
                RaisePropertyChanged("AddDevice");
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
            AddDevice = new Device();
            AddDeviceCommand = new RelayCommand<Device>(
                (p) => 
                {
                    if (ValidateIPv4(AddDevice.DEVICE_IP) && !String.IsNullOrEmpty(AddDevice.DEVICE_NAME))
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
                    AddDevice.DEVICE_STATUS = DeviceStatus.Pending.ToString();
                    if(SqliteDataAccess.InsertDataDevice(AddDevice))
                    {
                        AddDeviceStatus = "Succeed";
                    }
                    else
                    {
                        AddDeviceStatus = "Unsucceed";
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
