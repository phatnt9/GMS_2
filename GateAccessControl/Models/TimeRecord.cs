using System;
using System.ComponentModel;

namespace GateAccessControl
{
    public class TimeRecord : INotifyPropertyChanged
    {
        private int _timeCheckId;
        private DateTime _timeCheckTime;
        private string _pinno;
        private string _deviceIp;

        public TimeRecord()
        {
        }

        public TimeRecord(string ip, string pinno, DateTime checkTime)
        {
            TIMECHECK_ID = 0;
            DEVICE_IP = ip;
            PIN_NO = pinno;
            TIMECHECK_TIME = checkTime;
        }

        public int TIMECHECK_ID
        {
            get => _timeCheckId;
            set
            {
                _timeCheckId = value;
                OnPropertyChanged("TIMECHECK_ID");
            }
        }

        public DateTime TIMECHECK_TIME
        {
            get => _timeCheckTime;
            set
            {
                _timeCheckTime = value;
                OnPropertyChanged("TIMECHECK_TIME");
            }
        }

        public String PIN_NO
        {
            get => _pinno;
            set
            {
                _pinno = value;
                OnPropertyChanged("PIN_NO");
            }
        }

        public String DEVICE_IP
        {
            get => _deviceIp;
            set
            {
                _deviceIp = value;
                OnPropertyChanged("DEVICE_IP");
            }
        }

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged Members
    }
}