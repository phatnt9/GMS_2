using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Input;

namespace GateAccessControl
{
    class EditDeviceViewModels : ViewModelBase
    {
        private bool? _dialogResult;
        private string _editDeviceStatus;
        private Device _editDevice;

        public Device EditDevice
        {
            get
            {
                return _editDevice;
            }
            set
            {
                _editDevice = value;
                RaisePropertyChanged("EditDevice");
            }
        }

        public String EditDeviceStatus
        {
            get
            {
                return _editDeviceStatus;
            }
            set
            {
                _editDeviceStatus = value;
                RaisePropertyChanged("EditDeviceStatus");
            }
        }
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
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ICommand EditDeviceCommand { get; set; }
        public ICommand CloseEditDeviceCommand { get; set; }


        public EditDeviceViewModels(Device device)
        {
            EditDevice = device;

            EditDeviceCommand = new RelayCommand<Device>(
              (p) =>
              {
                  if (ValidateIPv4(EditDevice.DEVICE_IP) && !String.IsNullOrEmpty(EditDevice.DEVICE_NAME))
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
                  SaveDevice(EditDevice);
              });

            CloseEditDeviceCommand = new RelayCommand<Device>(
              (p) =>
              {
                  return true;
              },
              (p) =>
              {
                  CloseWindow();
              });
        }

        private void SaveDevice(Device p)
        {
            if (SqliteDataAccess.UpdateDevice(p))
            {
                EditDeviceStatus = "Succeed";
            }
            else
            {
                EditDeviceStatus = "Unsucceed";
                List<Device> reloadDevice = SqliteDataAccess.LoadDevices(EditDevice.DEVICE_ID);
                foreach (Device d in reloadDevice)
                {
                    EditDevice.DEVICE_NAME = d.DEVICE_NAME;
                    EditDevice.DEVICE_IP = d.DEVICE_IP;
                    EditDevice.DEVICE_NOTE = d.DEVICE_NOTE;
                }
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
