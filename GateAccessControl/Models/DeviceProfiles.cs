using System;
using System.ComponentModel;

namespace GateAccessControl
{
    public class DeviceProfile : INotifyPropertyChanged
    {
        public DeviceProfile()
        {
        }

        public DeviceProfile(Profile p)
        {
            PROFILE_ID = 0;
            PIN_NO = p.pinno;
            AD_NO = p.AD_NO;
            PROFILE_NAME = p.PROFILE_NAME;
            CLASS_NAME = p.CLASS_NAME;
            SUB_CLASS = p.SUB_CLASS;
            GENDER = p.GENDER;
            DOB = p.DOB;
            DISU = p.DISU;
            EMAIL = p.EMAIL;
            ADDRESS = p.ADDRESS;
            PHONE = p.PHONE;
            PROFILE_STATUS = p.PROFILE_STATUS;
            IMAGE = p.IMAGE;
            DATE_TO_LOCK = p.DATE_TO_LOCK;
            CHECK_DATE_TO_LOCK = p.CHECK_DATE_TO_LOCK;
            LICENSE_PLATE = p.LICENSE_PLATE;
            DATE_CREATED = p.DATE_CREATED;
            DATE_MODIFIED = p.DATE_MODIFIED;
            SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
            CLIENT_STATUS = GlobalConstant.ClientStatus.Unknow.ToString();
            ACTIVE_TIME = "00:00-23:59-00:00-23:59";
        }

        public void CloneDataFromProfile(Profile p)
        {
            PIN_NO = p.pinno;
            AD_NO = p.AD_NO;
            PROFILE_NAME = p.PROFILE_NAME;
            CLASS_NAME = p.CLASS_NAME;
            SUB_CLASS = p.SUB_CLASS;
            GENDER = p.GENDER;
            DOB = p.DOB;
            DISU = p.DISU;
            EMAIL = p.EMAIL;
            ADDRESS = p.ADDRESS;
            PHONE = p.PHONE;
            //PROFILE_STATUS = p.PROFILE_STATUS;
            IMAGE = p.IMAGE;
            DATE_TO_LOCK = p.DATE_TO_LOCK;
            CHECK_DATE_TO_LOCK = p.CHECK_DATE_TO_LOCK;
            LICENSE_PLATE = p.LICENSE_PLATE;
            DATE_CREATED = p.DATE_CREATED;
            DATE_MODIFIED = p.DATE_MODIFIED;
        }

        public int PROFILE_ID
        {
            get => _profileId;
            set
            {
                _profileId = value;
                OnPropertyChanged("PROFILE_ID");
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

        public String AD_NO
        {
            get => _adno;
            set
            {
                _adno = value;
                OnPropertyChanged("AD_NO");
            }
        }

        public String PROFILE_NAME
        {
            get => _profileName;
            set
            {
                _profileName = value;
                OnPropertyChanged("PROFILE_NAME");
            }
        }

        public String CLASS_NAME
        {
            get => _className;
            set
            {
                _className = value;
                OnPropertyChanged("CLASS_NAME");
            }
        }

        public String SUB_CLASS
        {
            get => _subClass;
            set
            {
                _subClass = value;
                OnPropertyChanged("SUB_CLASS");
            }
        }

        public String GENDER
        {
            get => _gender;
            set
            {
                _gender = value;
                OnPropertyChanged("GENDER");
            }
        }

        public DateTime DOB
        {
            get => _dob;
            set
            {
                _dob = value;
                OnPropertyChanged("DOB");
            }
        }

        public DateTime DISU
        {
            get => _disu;
            set
            {
                _disu = value;
                OnPropertyChanged("DISU");
            }
        }

        public String EMAIL
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged("EMAIL");
            }
        }

        public String ADDRESS
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged("ADDRESS");
            }
        }

        public String PHONE
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged("PHONE");
            }
        }

        public String PROFILE_STATUS
        {
            get => _profileStatus;
            set
            {
                _profileStatus = value;
                OnPropertyChanged("PROFILE_STATUS");
            }
        }

        public String IMAGE
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged("IMAGE");
            }
        }

        public DateTime DATE_TO_LOCK
        {
            get => _dateToLock;
            set
            {
                _dateToLock = value;
                OnPropertyChanged("DATE_TO_LOCK");
            }
        }

        public bool CHECK_DATE_TO_LOCK
        {
            get => _checkDateToLock;
            set
            {
                _checkDateToLock = value;
                OnPropertyChanged("CHECK_DATE_TO_LOCK");
            }
        }

        public String LICENSE_PLATE
        {
            get => _licensePlate;
            set
            {
                _licensePlate = value;
                OnPropertyChanged("LICENSE_PLATE");
            }
        }

        public DateTime DATE_CREATED
        {
            get => _dateCreated;
            set
            {
                _dateCreated = value;
                OnPropertyChanged("DATE_CREATED");
            }
        }

        public DateTime DATE_MODIFIED
        {
            get => _dateModified;
            set
            {
                _dateModified = value;
                OnPropertyChanged("DATE_MODIFIED");
            }
        }

        public String SERVER_STATUS
        {
            get => _serverStatus;
            set
            {
                _serverStatus = value;
                OnPropertyChanged("SERVER_STATUS");
            }
        }

        public String CLIENT_STATUS
        {
            get => _clientStatus;
            set
            {
                _clientStatus = value;
                OnPropertyChanged("CLIENT_STATUS");
            }
        }

        public String ACTIVE_TIME
        {
            get => _activeTime;
            set
            {
                _activeTime = value;
                OnPropertyChanged("ACTIVE_TIME");
            }
        }

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

        private DateTime _dateCreated;
        private DateTime _dateModified;

        private string _serverStatus;
        private string _clientStatus;
        private string _activeTime;

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