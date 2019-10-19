using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateAccessControl
{
    public class DeviceProfiles : INotifyPropertyChanged
    {
        public DeviceProfiles()
        {

        }

        public DeviceProfiles(Profile p)
        {
            PROFILE_ID = 0;
            PIN_NO = p.PIN_NO;
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
            LOCK_DATE = p.LOCK_DATE;
            DATE_TO_LOCK = p.DATE_TO_LOCK;
            CHECK_DATE_TO_LOCK = p.CHECK_DATE_TO_LOCK;
            LICENSE_PLATE = p.LICENSE_PLATE;
            DATE_CREATED = p.DATE_CREATED;
            DATE_MODIFIED = p.DATE_MODIFIED;
            SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
            CLIENT_STATUS = GlobalConstant.ClientStatus.Unknow.ToString();
            ACTIVE_TIME = "00:00,23:59;00:00,23:59";
        }

        public void CloneDataFromProfile(Profile p)
        {
            PIN_NO = p.PIN_NO;
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
            LOCK_DATE = p.LOCK_DATE;
            DATE_TO_LOCK = p.DATE_TO_LOCK;
            CHECK_DATE_TO_LOCK = p.CHECK_DATE_TO_LOCK;
            LICENSE_PLATE = p.LICENSE_PLATE;
            DATE_CREATED = p.DATE_CREATED;
            DATE_MODIFIED = p.DATE_MODIFIED;
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
        public DateTime LOCK_DATE
        {
            get
            {
                return _lockDate;
            }
            set
            {
                _lockDate = value;
                OnPropertyChanged("LOCK_DATE");
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
        public String SERVER_STATUS
        {
            get
            {
                return _serverStatus;
            }
            set
            {
                _serverStatus = value;
                OnPropertyChanged("SERVER_STATUS");
            }
        }
        public String CLIENT_STATUS
        {
            get
            {
                return _clientStatus;
            }
            set
            {
                _clientStatus = value;
                OnPropertyChanged("CLIENT_STATUS");
            }
        }
        public String ACTIVE_TIME
        {
            get
            {
                return _activeTime;
            }
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


        private DateTime _lockDate;
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
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
