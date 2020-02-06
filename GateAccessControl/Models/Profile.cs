using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            image = "default.png";
        }

        public void AddDeviceId(int deviceId)
        {
            List<int> listDeviceId = new List<int>();
            if (String.IsNullOrEmpty(list_device_id))
            {
                list_device_id += deviceId + ",";
            }
            else
            {
                string[] listVar = this.list_device_id.Split(',');
                foreach (string var in listVar)
                {
                    int temp;
                    Int32.TryParse(var, out temp);
                    if (temp != 0)
                    {
                        listDeviceId.Add(temp);
                    }
                }
                if (!listDeviceId.Contains(deviceId))
                {
                    list_device_id += deviceId + ",";
                }
            }
        }

        public async Task RemoveDeviceId(int removeDeviceId)
        {
            List<Device> listDevices = await SqliteDataAccess.LoadDevicesAsync(0);
            List<int> listDeviceId = new List<int>();
            if (!string.IsNullOrEmpty(list_device_id))
            {
                string[] listVar = list_device_id.Split(',');
                foreach (string var in listVar)
                {
                    int temp;
                    int.TryParse(var, out temp);
                    if (temp != 0)
                    {
                        listDeviceId.Add(temp);
                    }
                }
                list_device_id = "";
                foreach (int id in listDeviceId)
                {
                    if (id != removeDeviceId && CheckDeviceAlive(id, listDevices))
                    {
                        list_device_id += id + ",";
                    }
                }
            }
        }

        private bool CheckDeviceAlive(int id, List<Device> devices)
        {
            foreach (Device item in devices)
            {
                if (item.deviceId == id)
                {
                    return true;
                }
            }
            return false;
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

        public String list_device_id
        {
            get => _listDeviceId;
            set
            {
                _listDeviceId = value;
                OnPropertyChanged("list_device_id");
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