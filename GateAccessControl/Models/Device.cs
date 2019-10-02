using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateAccessControl
{
    public class Device : INotifyPropertyChanged
    {
        private int _deviceId;
        private string _deviceIp;
        private string _deviceName;
        private string _deviceStatus;
        private string _deviceNote;

        public int DEVICE_ID
        {
            get
            {
                return _deviceId;
            }
            set
            {
                _deviceId = value;
                OnPropertyChanged("DEVICE_ID");
            }
        }
        public String DEVICE_IP
        {
            get
            {
                return _deviceIp;
            }
            set
            {
                _deviceIp = value;
                OnPropertyChanged("DEVICE_IP");
            }
        }
        public String DEVICE_NAME
        {
            get
            {
                return _deviceName;
            }
            set
            {
                _deviceName = value;
                OnPropertyChanged("DEVICE_NAME");
            }
        }
        public String DEVICE_STATUS
        {
            get
            {
                return _deviceStatus;
            }
            set
            {
                _deviceStatus = value;
                OnPropertyChanged("DEVICE_STATUS");
            }
        }
        public String DEVICE_NOTE
        {
            get
            {
                return _deviceNote;
            }
            set
            {
                _deviceNote = value;
                OnPropertyChanged("DEVICE_NOTE");
            }
        }

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
