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
            profileId = 0;
            pinno = p.pinno;
            adno = p.adno;
            profileName = p.profileName;
            className = p.className;
            subClass = p.subClass;
            gender = p.gender;
            dob = p.dob;
            disu = p.disu;
            email = p.email;
            address = p.address;
            phone = p.phone;
            profileStatus = p.profileStatus;
            image = p.image;
            date_to_lock = p.date_to_lock;
            check_date_to_lock = p.check_date_to_lock;
            license_plate = p.license_plate;
            date_created = p.date_created;
            date_modified = p.date_modified;
            server_status = GlobalConstant.ServerStatus.Add.ToString();
            client_status = GlobalConstant.ClientStatus.Unknow.ToString();
            active_time = "00:00-23:59-00:00-23:59";
        }

        public void CloneDataFromProfile(Profile p)
        {
            pinno = p.pinno;
            adno = p.adno;
            profileName = p.profileName;
            className = p.className;
            subClass = p.subClass;
            gender = p.gender;
            dob = p.dob;
            disu = p.disu;
            email = p.email;
            address = p.address;
            phone = p.phone;
            //PROFILE_STATUS = p.PROFILE_STATUS;
            image = p.image;
            date_to_lock = p.date_to_lock;
            check_date_to_lock = p.check_date_to_lock;
            license_plate = p.license_plate;
            date_created = p.date_created;
            date_modified = p.date_modified;
        }

        public int profileId
        {
            get => _profileId;
            set
            {
                _profileId = value;
                OnPropertyChanged("profileId");
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

        public String adno
        {
            get => _adno;
            set
            {
                _adno = value;
                OnPropertyChanged("adno");
            }
        }

        public String profileName
        {
            get => _profileName;
            set
            {
                _profileName = value;
                OnPropertyChanged("profileName");
            }
        }

        public String className
        {
            get => _className;
            set
            {
                _className = value;
                OnPropertyChanged("className");
            }
        }

        public String subClass
        {
            get => _subClass;
            set
            {
                _subClass = value;
                OnPropertyChanged("subClass");
            }
        }

        public String gender
        {
            get => _gender;
            set
            {
                _gender = value;
                OnPropertyChanged("gender");
            }
        }

        public DateTime dob
        {
            get => _dob;
            set
            {
                _dob = value;
                OnPropertyChanged("dob");
            }
        }

        public DateTime disu
        {
            get => _disu;
            set
            {
                _disu = value;
                OnPropertyChanged("disu");
            }
        }

        public String email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged("email");
            }
        }

        public String address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged("address");
            }
        }

        public String phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged("phone");
            }
        }

        public String profileStatus
        {
            get => _profileStatus;
            set
            {
                _profileStatus = value;
                OnPropertyChanged("profileStatus");
            }
        }

        public String image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged("image");
            }
        }

        public DateTime date_to_lock
        {
            get => _dateToLock;
            set
            {
                _dateToLock = value;
                OnPropertyChanged("date_to_lock");
            }
        }

        public bool check_date_to_lock
        {
            get => _checkDateToLock;
            set
            {
                _checkDateToLock = value;
                OnPropertyChanged("check_date_to_lock");
            }
        }

        public String license_plate
        {
            get => _licensePlate;
            set
            {
                _licensePlate = value;
                OnPropertyChanged("license_plate");
            }
        }

        public DateTime date_created
        {
            get => _dateCreated;
            set
            {
                _dateCreated = value;
                OnPropertyChanged("date_created");
            }
        }

        public DateTime date_modified
        {
            get => _dateModified;
            set
            {
                _dateModified = value;
                OnPropertyChanged("date_modified");
            }
        }

        public String server_status
        {
            get => _serverStatus;
            set
            {
                _serverStatus = value;
                OnPropertyChanged("server_status");
            }
        }

        public String client_status
        {
            get => _clientStatus;
            set
            {
                _clientStatus = value;
                OnPropertyChanged("client_status");
            }
        }

        public String active_time
        {
            get => _activeTime;
            set
            {
                _activeTime = value;
                OnPropertyChanged("active_time");
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