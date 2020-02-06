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
            timeCheckId = 0;
            deviceIp = ip;
            this.pinno = pinno;
            timeCheck = checkTime;
        }

        public int timeCheckId
        {
            get => _timeCheckId;
            set
            {
                _timeCheckId = value;
                OnPropertyChanged("timeCheckId");
            }
        }

        public DateTime timeCheck
        {
            get => _timeCheckTime;
            set
            {
                _timeCheckTime = value;
                OnPropertyChanged("timeCheck");
            }
        }

        public String pinno
        {
            get => _pinno;
            set
            {
                _pinno = value;
                OnPropertyChanged("pinno");
            }
        }

        public String deviceIp
        {
            get => _deviceIp;
            set
            {
                _deviceIp = value;
                OnPropertyChanged("deviceIp");
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