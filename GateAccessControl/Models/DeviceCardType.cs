using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateAccessControl
{
    public class DeviceCardType : INotifyPropertyChanged
    {
        private int _deviceClassId;
        private int _deviceId;
        private int _classId;
        private bool _checkStatus;

        public int DEVICE_CLASS_ID
        {
            get
            {
                return _deviceClassId;
            }
            set
            {
                _deviceClassId = value;
                OnPropertyChanged("DEVICE_CLASS_ID");
            }
        }
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
        public int CLASS_ID
        {
            get
            {
                return _classId;
            }
            set
            {
                _classId = value;
                OnPropertyChanged("CLASS_ID");
            }
        }
        public bool CHECK_STATUS
        {
            get
            {
                return _checkStatus;
            }
            set
            {
                _checkStatus = value;
                OnPropertyChanged("CHECK_STATUS");
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
