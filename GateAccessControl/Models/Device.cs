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
        private DeviceItem _deviceItem;
        private int _syncProgressValue;
        public BackgroundWorker SyncWorker;

        public void SyncDeviceProfiles(List<DeviceProfile> profiles)
        {
            List<DeviceProfile> syncList = new List<DeviceProfile>(profiles);
            SyncWorker = new BackgroundWorker();
            SyncWorker.WorkerSupportsCancellation = true;
            SyncWorker.WorkerReportsProgress = true;
            SyncWorker.DoWork += SyncWorker_DoWork;
            SyncWorker.RunWorkerCompleted += SyncWorker_RunWorkerCompleted;
            SyncWorker.ProgressChanged += SyncWorker_ProgressChanged;
            SyncWorker.RunWorkerAsync(profiles);
        }

        private void SyncWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SyncProgressValue = e.ProgressPercentage;
        }

        private void SyncWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // handle the error
                //PgbStatus = AppStatus.Error;
            }
            else if (e.Cancelled)
            {
                // handle cancellation
                //PgbStatus = AppStatus.Cancelled;
            }
            else
            {
                //PgbStatus = AppStatus.Completed;
            }
            DeviceItem.IsSendingProfiles = false;
            SyncProgressValue = 0;
        }

        private void SyncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool remainProfiles = true;
            DeviceItem.IsSendingProfiles = true;
            List<DeviceProfile> profiles = e.Argument as List<DeviceProfile>;
            //Device device = (Device)genericlist[1];
            for (int i = 0; i < profiles.Count; i++)
            {
                if (i == (profiles.Count - 1))
                {
                    remainProfiles = false;
                }
                DeviceProfile deviceProfileToSend = profiles[i];

                if ((deviceProfileToSend.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Active.ToString())) &&
                        (deviceProfileToSend.SERVER_STATUS.Equals(GlobalConstant.ServerStatus.None.ToString())))
                {
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);

                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_UPDATE;
                    if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                    {
                        serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    }

                    List<DeviceProfile> sendList = new List<DeviceProfile>();
                    sendList.Add(deviceProfileToSend);
                    if (DeviceItem.SendDeviceProfile(DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
                            deviceProfileToSend.CLIENT_STATUS = GlobalConstant.ClientStatus.Deleted.ToString();
                        }
                        else
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                        }
                        SqliteDataAccess.UpdateDeviceProfile(DEVICE_ID, deviceProfileToSend);
                    }
                    else
                    {
                        deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                    }
                    if (SyncWorker.CancellationPending)
                    {
                        break;
                    }
                    continue;
                }

                if ((deviceProfileToSend.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Active.ToString())) &&
                        (deviceProfileToSend.SERVER_STATUS.Equals(GlobalConstant.ServerStatus.Add.ToString())))
                {
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);

                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_ADD;
                    if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                    {
                        serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    }

                    List<DeviceProfile> sendList = new List<DeviceProfile>();
                    sendList.Add(deviceProfileToSend);
                    if (DeviceItem.SendDeviceProfile(DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
                            deviceProfileToSend.CLIENT_STATUS = GlobalConstant.ClientStatus.Deleted.ToString();
                        }
                        else
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                        }
                        SqliteDataAccess.UpdateDeviceProfile(DEVICE_ID, deviceProfileToSend);
                    }
                    else
                    {
                        deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
                    }
                    if (SyncWorker.CancellationPending)
                    {
                        break;
                    }
                    continue;
                }

                if ((deviceProfileToSend.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Active.ToString())) &&
                        (deviceProfileToSend.SERVER_STATUS.Equals(GlobalConstant.ServerStatus.Update.ToString())))
                {
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);

                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_UPDATE;
                    if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                    {
                        serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    }

                    List<DeviceProfile> sendList = new List<DeviceProfile>();
                    sendList.Add(deviceProfileToSend);
                    if (DeviceItem.SendDeviceProfile(DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
                            deviceProfileToSend.CLIENT_STATUS = GlobalConstant.ClientStatus.Deleted.ToString();
                        }
                        else
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                        }
                        SqliteDataAccess.UpdateDeviceProfile(DEVICE_ID, deviceProfileToSend);
                    }
                    else
                    {
                        deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Update.ToString();
                    }
                    if (SyncWorker.CancellationPending)
                    {
                        break;
                    }
                    continue;
                }

                if ((deviceProfileToSend.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Active.ToString())) &&
                        (deviceProfileToSend.SERVER_STATUS.Equals(GlobalConstant.ServerStatus.Remove.ToString())))
                {
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);

                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                    {
                        serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    }

                    List<DeviceProfile> sendList = new List<DeviceProfile>();
                    sendList.Add(deviceProfileToSend);
                    if (DeviceItem.SendDeviceProfile(DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                            deviceProfileToSend.CLIENT_STATUS = GlobalConstant.ClientStatus.Deleted.ToString();
                        }
                        else
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                        }
                        SqliteDataAccess.UpdateDeviceProfile(DEVICE_ID, deviceProfileToSend);
                    }
                    else
                    {
                        deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Remove.ToString();
                    }
                    if (SyncWorker.CancellationPending)
                    {
                        break;
                    }
                    continue;
                }

                if ((deviceProfileToSend.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Suspended.ToString())) &&
                        (deviceProfileToSend.SERVER_STATUS.Equals(GlobalConstant.ServerStatus.Remove.ToString())))
                {
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);

                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                    {
                        serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    }

                    List<DeviceProfile> sendList = new List<DeviceProfile>();
                    sendList.Add(deviceProfileToSend);
                    if (DeviceItem.SendDeviceProfile(DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                            deviceProfileToSend.CLIENT_STATUS = GlobalConstant.ClientStatus.Deleted.ToString();
                        }
                        else
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                        }
                        SqliteDataAccess.UpdateDeviceProfile(DEVICE_ID, deviceProfileToSend);
                    }
                    else
                    {
                        deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Remove.ToString();
                    }
                    if (SyncWorker.CancellationPending)
                    {
                        break;
                    }
                    continue;
                }

                if ((deviceProfileToSend.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (deviceProfileToSend.SERVER_STATUS.Equals(GlobalConstant.ServerStatus.Add.ToString()))))
                {
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);

                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_ADD;
                    if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                    {
                        serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    }

                    List<DeviceProfile> sendList = new List<DeviceProfile>();
                    sendList.Add(deviceProfileToSend);
                    if (DeviceItem.SendDeviceProfile(DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        if (deviceProfileToSend.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Delete.ToString()))
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
                            deviceProfileToSend.CLIENT_STATUS = GlobalConstant.ClientStatus.Deleted.ToString();
                        }
                        else
                        {
                            deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                            deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                        }
                        SqliteDataAccess.UpdateDeviceProfile(DEVICE_ID, deviceProfileToSend);
                    }
                    else
                    {
                        deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Add.ToString();
                    }
                    if (SyncWorker.CancellationPending)
                    {
                        break;
                    }
                    continue;
                }
            }
        }

        public Device()
        {
            DeviceItem = new DeviceItem();
            SyncProgressValue = 0;
        }
        public DeviceItem DeviceItem
        {
            get
            {
                return _deviceItem;
            }
            set
            {
                _deviceItem = value;
                OnPropertyChanged("DeviceItem");
            }
        }

        public int SyncProgressValue
        {
            get => _syncProgressValue;
            set
            {
                _syncProgressValue = value;
                OnPropertyChanged("SyncProgressValue");
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
