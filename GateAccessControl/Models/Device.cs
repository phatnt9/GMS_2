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
        private int _numberOfSyncingDevices;
        public BackgroundWorker SyncWorker;

        public void SyncDeviceProfiles(List<DeviceProfiles> profiles,ref int NoSyncingDevice)
        {
            NumberOfSyncingDevices = NoSyncingDevice;
            List<DeviceProfiles> syncList = new List<DeviceProfiles>(profiles);
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
            NumberOfSyncingDevices--;
            SyncProgressValue = 0;
        }

        private void SyncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            NumberOfSyncingDevices++;
            bool remainProfiles = true;
            DeviceItem.IsSendingProfiles = true;
            List<DeviceProfiles> profiles = e.Argument as List<DeviceProfiles>;
            //Device device = (Device)genericlist[1];
            for (int i = 0; i < profiles.Count; i++)
            {
                if (i == (profiles.Count - 1))
                {
                    remainProfiles = false;
                }
                DeviceProfiles deviceProfileToSend = profiles[i];

                if ((deviceProfileToSend.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (deviceProfileToSend.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString()))
                {
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);
                    deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                    deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Syncing.ToString();

                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_ADD;
                    List<DeviceProfiles> sendList = new List<DeviceProfiles>();
                    sendList.Add(deviceProfileToSend);
                    if (DeviceItem.SendDeviceProfile(DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                        SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + DEVICE_ID, deviceProfileToSend);
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

                if ((deviceProfileToSend.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (deviceProfileToSend.SERVER_STATUS == GlobalConstant.ServerStatus.Update.ToString()))
                {
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);
                    deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                    deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Syncing.ToString();

                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_UPDATE;
                    List<DeviceProfiles> sendList = new List<DeviceProfiles>();
                    sendList.Add(deviceProfileToSend);
                    if (DeviceItem.SendDeviceProfile(DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                        SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + DEVICE_ID, deviceProfileToSend);
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

                if ((deviceProfileToSend.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                        (deviceProfileToSend.SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString()))
                {
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);
                    deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                    deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Syncing.ToString();

                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    List<DeviceProfiles> sendList = new List<DeviceProfiles>();
                    sendList.Add(deviceProfileToSend);
                    if (DeviceItem.SendDeviceProfile(DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                        SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + DEVICE_ID, deviceProfileToSend);
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

                if ((deviceProfileToSend.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (deviceProfileToSend.SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString()))
                {
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);
                    deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                    deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Syncing.ToString();

                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                    List<DeviceProfiles> sendList = new List<DeviceProfiles>();
                    sendList.Add(deviceProfileToSend);
                    if (DeviceItem.SendDeviceProfile(DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                        SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + DEVICE_ID, deviceProfileToSend);
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

                if ((deviceProfileToSend.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                        (deviceProfileToSend.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString()))
                {
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profiles.Count);
                    deviceProfileToSend.PROFILE_STATUS = GlobalConstant.ProfileStatus.Active.ToString();
                    deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.Syncing.ToString();

                    DeviceItem.SERVERRESPONSE serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_ADD;
                    List<DeviceProfiles> sendList = new List<DeviceProfiles>();
                    sendList.Add(deviceProfileToSend);
                    if (DeviceItem.SendDeviceProfile(DEVICE_IP, serRes, sendList, remainProfiles))
                    {
                        deviceProfileToSend.SERVER_STATUS = GlobalConstant.ServerStatus.None.ToString();
                        SqliteDataAccess.UpdateDataDeviceProfiles("DT_DEVICE_PROFILES_" + DEVICE_ID, deviceProfileToSend);
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

        public int NumberOfSyncingDevices
        {
            get => _numberOfSyncingDevices;
            set
            {
                _numberOfSyncingDevices = value;
                OnPropertyChanged("NumberOfSyncingDevices");
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
