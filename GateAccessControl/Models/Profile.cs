using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateAccessControl
{
    public class Profile : INotifyPropertyChanged
    {
        private int _profileId;
        private string _pinno;
        private string _adno;
        private string _profileName;
        private string _className;
        private string _subClass;
        private string _gender;
        private DateTime _dob;
        private DateTime _disu;

        private string _email;
        private string _address;
        private string _phone;
        private string _profileStatus;
        private string _image;
        
        private DateTime _dateToLock;

        private bool _checkDateToLock;
        private string _licensePlate;
        private string _listDeviceId;

        private DateTime _dateCreated;
        private DateTime _dateModified;

        public Profile()
        {
            IMAGE = "default.png";
        }

        public void AddDeviceId(int deviceId)
        {
            List<int> listDeviceId = new List<int>();
            if (LIST_DEVICE_ID == null)
            {
                LIST_DEVICE_ID += deviceId + ",";
            }
            else
            {
                string[] listVar = this.LIST_DEVICE_ID.Split(',');
                foreach (string var in listVar)
                {
                    int temp;
                    Int32.TryParse(var, out temp);
                    if(temp != 0)
                    {
                        listDeviceId.Add(temp);
                    }
                }
                if (!listDeviceId.Contains(deviceId))
                {
                    LIST_DEVICE_ID += deviceId + ",";
                }
            }
        }
        public void RemoveDeviceId(int removeDeviceId)
        {
            List<Device> listDevices = SqliteDataAccess.LoadDevices(0);
            List<int> listDeviceId = new List<int>();
            if (!String.IsNullOrEmpty(LIST_DEVICE_ID))
            {
                string[] listVar = this.LIST_DEVICE_ID.Split(',');
                foreach (string var in listVar)
                {
                    int temp;
                    Int32.TryParse(var, out temp);
                    if (temp != 0)
                    {
                        listDeviceId.Add(temp);
                    }
                }
                LIST_DEVICE_ID = "";
                foreach (int id in listDeviceId)
                {
                    if(id != removeDeviceId && CheckDeviceAlive(id, listDevices))
                    {
                        LIST_DEVICE_ID += id + ",";
                    }
                }
            }
        }

        private bool CheckDeviceAlive(int id, List<Device> devices)
        {
            foreach(Device item in devices)
            {
                if(item.DEVICE_ID == id)
                {
                    return true;
                }
            }
            return false;
        }

        public int PROFILE_ID
        {
            get
            {
                return _profileId;
            }
            set
            {
                _profileId = value;
                OnPropertyChanged("PROFILE_ID");
            }
        }
        public String PIN_NO
        {
            get
            {
                return _pinno;
            }
            set
            {
                _pinno = value;
                OnPropertyChanged("PIN_NO");
            }
        }
        public String AD_NO
        {
            get
            {
                return _adno;
            }
            set
            {
                _adno = value;
                OnPropertyChanged("AD_NO");
            }
        }
        public String PROFILE_NAME
        {
            get
            {
                return _profileName;
            }
            set
            {
                _profileName = value;
                OnPropertyChanged("PROFILE_NAME");
            }
        }
        public String CLASS_NAME
        {
            get
            {
                return _className;
            }
            set
            {
                _className = value;
                OnPropertyChanged("CLASS_NAME");
            }
        }
        public String SUB_CLASS
        {
            get
            {
                return _subClass;
            }
            set
            {
                _subClass = value;
                OnPropertyChanged("SUB_CLASS");
            }
        }
        public String GENDER
        {
            get
            {
                return _gender;
            }
            set
            {
                _gender = value;
                OnPropertyChanged("GENDER");
            }
        }
        public DateTime DOB
        {
            get
            {
                return _dob;
            }
            set
            {
                _dob = value;
                OnPropertyChanged("DOB");
            }
        }
        public DateTime DISU
        {
            get
            {
                return _disu;
            }
            set
            {
                _disu = value;
                OnPropertyChanged("DISU");
            }
        }
        public String EMAIL
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                OnPropertyChanged("EMAIL");
            }
        }
        public String ADDRESS
        {
            get
            {
                return _address;
            }
            set
            {
                _address = value;
                OnPropertyChanged("ADDRESS");
            }
        }
        public String PHONE
        {
            get
            {
                return _phone;
            }
            set
            {
                _phone = value;
                OnPropertyChanged("PHONE");
            }
        }
        public String PROFILE_STATUS
        {
            get
            {
                return _profileStatus;
            }
            set
            {
                _profileStatus = value;
                OnPropertyChanged("PROFILE_STATUS");
            }
        }
        public String IMAGE
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                OnPropertyChanged("IMAGE");
            }
        }
        public DateTime DATE_TO_LOCK
        {
            get
            {
                return _dateToLock;
            }
            set
            {
                _dateToLock = value;
                OnPropertyChanged("DATE_TO_LOCK");
            }
        }
        public bool CHECK_DATE_TO_LOCK
        {
            get
            {
                return _checkDateToLock;
            }
            set
            {
                _checkDateToLock = value;
                OnPropertyChanged("CHECK_DATE_TO_LOCK");
            }
        }
        public String LICENSE_PLATE
        {
            get
            {
                return _licensePlate;
            }
            set
            {
                _licensePlate = value;
                OnPropertyChanged("LICENSE_PLATE");
            }
        }
        public DateTime DATE_CREATED
        {
            get
            {
                return _dateCreated;
            }
            set
            {
                _dateCreated = value;
                OnPropertyChanged("DATE_CREATED");
            }
        }
        public DateTime DATE_MODIFIED
        {
            get
            {
                return _dateModified;
            }
            set
            {
                _dateModified = value;
                OnPropertyChanged("DATE_MODIFIED");
            }
        }
        public String LIST_DEVICE_ID
        {
            get
            {
                return _listDeviceId;
            }
            set
            {
                _listDeviceId = value;
                OnPropertyChanged("LIST_DEVICE_ID");
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
