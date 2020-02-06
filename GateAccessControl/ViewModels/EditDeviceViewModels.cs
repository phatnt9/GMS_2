using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Input;

namespace GateAccessControl
{
    internal class EditDeviceViewModels : ViewModelBase
    {
        private bool? _dialogResult;
        private string _editDeviceStatus;
        private Device _editDevice;

        public Device EditDevice
        {
            get => _editDevice;
            set
            {
                _editDevice = value;
                RaisePropertyChanged("EditDevice");
            }
        }

        public String EditDeviceStatus
        {
            get => _editDeviceStatus;
            set
            {
                _editDeviceStatus = value;
                RaisePropertyChanged("EditDeviceStatus");
            }
        }

        public bool? DialogResult
        {
            get => _dialogResult;
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
                  if (ValidateIPv4(EditDevice.deviceIp) && !String.IsNullOrEmpty(EditDevice.deviceName))
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

        private async void SaveDevice(Device p)
        {
            if (await SqliteDataAccess.UpdateDeviceAsync(p))
            {
                EditDeviceStatus = "Succeed";
            }
            else
            {
                EditDeviceStatus = "Unsucceed";
                List<Device> reloadDevice = await SqliteDataAccess.LoadDevicesAsync(EditDevice.deviceId);
                foreach (Device d in reloadDevice)
                {
                    EditDevice.deviceName = d.deviceName;
                    EditDevice.deviceIp = d.deviceIp;
                    EditDevice.deviceNote = d.deviceNote;
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