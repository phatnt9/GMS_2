using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GateAccessControl
{
    internal class ProfileDevice : INotifyPropertyChanged
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int _deviceId;
        private string _deviceIp;
        private string _deviceName;
        private string _deviceNote;

        public ProfileDevice()
        {
        }

        public ProfileDevice(int id)
        {
            this.deviceId = id;
            Task getInfoTask = GetDeviceInfo();
        }

        public async Task GetDeviceInfo()
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;
                string url = $@"http://{serverIp}:{serverPort}/serverschool/load/deviceInf/?deviceId={deviceId}";

                Task<string> responseTask = SqliteDataAccess.client.GetStringAsync(url);
                var responseString = await responseTask;
                List<Device> listObjects = JsonConvert.DeserializeObject<List<Device>>(responseString);
                if (listObjects[0] != null)
                {
                    deviceIp = listObjects[0].deviceIp;
                    deviceName = listObjects[0].deviceName;
                    deviceNote = listObjects[0].deviceNote;
                    Console.WriteLine($"ProfileDevice successfully loaded! id {this.deviceId}");
                }
                else
                {
                    deviceIp = "unknow";
                    deviceName = "unknow";
                    deviceNote = "unknow";
                    Console.WriteLine($"ProfileDevice load failed! id {this.deviceId}");
                }
            }
            catch (Exception ex)
            {
                deviceIp = "unknow";
                deviceName = "unknow";
                deviceNote = "unknow";
                Console.WriteLine($"ProfileDevice load failed! id {this.deviceId}");
                logFile.Error(ex.Message);
                return;
            }
        }

        public int deviceId
        {
            get => _deviceId;
            set
            {
                _deviceId = value;
                OnPropertyChanged("deviceId");
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

        public String deviceName
        {
            get => _deviceName;
            set
            {
                _deviceName = value;
                OnPropertyChanged("deviceName");
            }
        }

        public String deviceNote
        {
            get => _deviceNote;
            set
            {
                _deviceNote = value;
                OnPropertyChanged("deviceNote");
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