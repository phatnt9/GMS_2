using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using WebSocketSharp;

namespace GateAccessControl
{
    public class DeviceItem :RosSocket
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public System.Timers.Timer checkAlive;


        public enum STATUSPROFILE
        {
            Ready,
            Updated,
            Updating,
            Failed,
            Error,
            Pending,
            Requesting,
        }

        public enum CLIENTCMD
        {
            CLIENT_READY = 1000,
            REQUEST_PROFILE_ADD = 110,
            REQUEST_PROFILE_UPDATE = 111,
            REQUEST_PROFILE_DELETE = 112,
            REQUEST_REG_PERSON_LIST = 120,
            REQUEST_SYNC_TIME = 130,
            CONFIRM_ADD_PROFILE_SUCCESS = 310,
            CONFIRM_UPDATE_PROFILE_SUCCESS = 311,
            CONFIRM_DELETE_PROFILE_SUCCESS = 312,
            CONFIRM_ADD_PROFILE_UNSUCCESS = 320,
            CONFIRM_UPDATE_PROFILE_UNSUCCESS = 321,
            CONFIRM_DELETE_PROFILE_UNSUCCESS = 322,
        }

        public enum SERVERRESPONSE
        {
            RESP_SUCCESS = 200,
            RESP_PROFILE_ADD = 210,
            RESP_PROFILE_UPDATE = 211,
            RESP_PROFILE_DELETE = 212,
            RESP_SYNC_TIME = 220,
            RESP_SEND_NEWPROFILE_IMMEDIATELY = 240,
            RESP_REQ_PERSONLIST_IMMEDIATELY = 250,
            RESP_DATAFORMAT_ERROR = 300,
            RESP_PERSONLIST_ERROR = 320,
        }

        //public enum

        public class JStringProfile
        {
            public int status;
            public List<Profile> data;
        }

        public class JStringDeviceProfile
        {
            public int status;
            public List<DeviceProfiles> data;
            public bool remainProfiles;
        }

        public class JStringClient
        {
            public int deviceId;
            public int status;
            public List<String> data;
        }

        public struct FLAGSTATUSCLIENT
        {
            public CLIENTCMD OnConfirmProfileSuccess;
        }

        private int publishdata;
        private int publishdataImg;

        //public event Action<String> MessageCallBack;
        public FLAGSTATUSCLIENT OnFlagStatusClient;

        private string _statusProfile = STATUSPROFILE.Pending.ToString();
        public String StatusProfile
        {
            get
            {
                return _statusProfile;
            }
            set
            {
                _statusProfile = value;
                OnPropertyChanged("StatusProfile");
            }
        }


        public DeviceItem()
        {
            WebSocketStatus = RosStatus.Disconnected.ToString();
            checkAlive = new System.Timers.Timer();
            checkAlive.Interval = 1000;
            checkAlive.Elapsed += CheckConnection;
            OnFlagStatusClient.OnConfirmProfileSuccess = CLIENTCMD.CLIENT_READY;
        }
        protected override void OnOpenedEvent()
        {
            try
            {
                createRosTerms();
                base.OnOpenedEvent();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Robot Control Error Send OnOpenedEvent");
                logFile.Error(ex.Message);
            }
        }

        private void CheckConnection(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (webSocket != null)
                {
                    if (webSocket.IsAlive)
                    {
                        WebSocketStatus = RosStatus.Connected.ToString();
                    }
                    else
                    {
                        WebSocketStatus = RosStatus.Connecting.ToString();
                    }
                }
                else
                {
                    WebSocketStatus = RosStatus.Disconnected.ToString();
                    checkAlive.Stop();
                }
            }
            catch
            {
                WebSocketStatus = RosStatus.Pending.ToString();
            }
        }
        public void createRosTerms()
        {
            int subscription_imagerequest = this.Subscribe("ReqImage", "std_msgs/String", ReqImgHandler);
            publishdataImg = this.Advertise("ServerRespImg", "sensor_msgs/CompressedImage");

            publishdata = this.Advertise("ServerPublish", "std_msgs/String");
            int subscription = this.Subscribe("ClientPublish", "std_msgs/String", DataHandler);
        }
        protected override void OnClosedEvent(object sender, CloseEventArgs e)
        {
            //checkAlive.Stop();
            base.OnClosedEvent(sender, e);
        }
        private void DataHandler(Message message)
        {
            StandardString standard = (StandardString)message;
            try
            {
                JObject stuff = JObject.Parse(standard.data);
                var status = (CLIENTCMD)((int)stuff["status"]);
                var ip = (string)stuff["ip"];
                switch (status)
                {
                    case CLIENTCMD.CONFIRM_ADD_PROFILE_SUCCESS:
                    {
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;
                    }
                    case CLIENTCMD.CONFIRM_UPDATE_PROFILE_SUCCESS:
                    {
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;
                    }
                    case CLIENTCMD.CONFIRM_DELETE_PROFILE_SUCCESS:
                    {
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;
                    }

                    case CLIENTCMD.CONFIRM_ADD_PROFILE_UNSUCCESS:
                    {
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;
                    }
                    case CLIENTCMD.CONFIRM_UPDATE_PROFILE_UNSUCCESS:
                    {
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;
                    }
                    case CLIENTCMD.CONFIRM_DELETE_PROFILE_UNSUCCESS:
                    {
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;
                    }
                    case CLIENTCMD.REQUEST_SYNC_TIME:
                    {
                        dynamic productTimeSync = new JObject();
                        productTimeSync.status = (int)SERVERRESPONSE.RESP_SYNC_TIME;
                        productTimeSync.data = DateTime.Now.Ticks;
                        StandardString msgTimeSync = new StandardString();
                        msgTimeSync.data = productTimeSync.ToString();
                        Publish(publishdata, msgTimeSync);
                        break;
                    }
                    case CLIENTCMD.REQUEST_REG_PERSON_LIST:
                    {
                        List<CheckinData> personList = new List<CheckinData>();
                        try
                        {
                            StatusProfile = "Received check-in records";
                            JObject dataClient = JObject.Parse(standard.data);
                            if (dataClient["data"].Count() > 0)
                            {
                                foreach (var result in dataClient["data"])
                                {
                                    string serialId = (string)result["serialId"];
                                    string tick = (string)result["tick"];
                                    CheckinData person = new CheckinData() { SERIAL_ID = serialId, TIMECHECK = tick };
                                    personList.Add(person);
                                }
                                if (personList.Count > 0)
                                {
                                    CheckinServer(ip, personList);
                                }
                            }
                            try
                            {
                                dynamic product = new JObject();
                                product.status = SERVERRESPONSE.RESP_SUCCESS;
                                StandardString msg = new StandardString();
                                msg.data = product.ToString();
                                Publish(publishdata, msg);
                            }
                            catch (Exception ex)
                            {
                                logFile.Error(ex.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                dynamic product = new JObject();
                                product.status = SERVERRESPONSE.RESP_PERSONLIST_ERROR;
                                StandardString msg = new StandardString();
                                msg.data = product.ToString();
                                Publish(publishdata, msg);
                                logFile.Error(ex.Message);
                            }
                            catch (Exception exc)
                            {
                                logFile.Error(exc.Message);
                            }
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }
        public void ReqImgHandler(Message message)
        {
            StandardString standard = (StandardString)message;
            dynamic stuff = JObject.Parse(standard.data);
            string ip = stuff.ip;
            string name = stuff.img;
            int percent = (int)stuff.percent;

            //Constant.mainWindowPointer.WriteLog("[" + ip + "]" + "  Image loaded: " + percent + "%"); //  [192.168.105.5]  Image loaded: 40%

            SensorCompressedImage imgdata = new SensorCompressedImage();

            try
            {
                //  BitmapImage img = new System.Windows.Media.Imaging.BitmapImage(new Uri(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + standard.data));
                String pathImg = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + name;
                if (!File.Exists(pathImg))
                {
                    //string test = @"pack://siteoforigin:,,,/Resources/" + "default.png";
                    //Image img = Image.FromFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + "default.png");
                    //Image img = null;
                    imgdata.format = "NoImage";
                    //imgdata.data = ImageToByte(img);
                    if (webSocket.IsAlive)
                    {
                        this.Publish(publishdataImg, imgdata);
                    }
                }
                else
                {
                    Image img = Image.FromFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + name);
                    Image resizeImg = new Bitmap(img, new Size(300, 400));
                    imgdata.format = name;
                    imgdata.data = ImageToByte(resizeImg);
                    if (webSocket.IsAlive)
                    {
                        this.Publish(publishdataImg, imgdata);
                    }
                }
            }
            catch
            {
                //Image img = Image.FromFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + "default.png");
                //Image img = null;
                imgdata.format = "NoImage";
                //imgdata.data = ImageToByte(img);
                if (webSocket.IsAlive)
                {
                    this.Publish(publishdataImg, imgdata);
                }
            }
        }

        public bool NextBool(Random r, int truePercentage = 50)
        {
            return r.NextDouble() < truePercentage / 100.0;
        }


        public bool SendDeviceProfile(string ip, SERVERRESPONSE serverRes, List<DeviceProfiles> DeviceProfileToSend, bool remainProfiles)
        {
            //Random r = new Random();
            //Thread.Sleep(100);
            //return NextBool(r, 90); //wrong
            try
            {
                JStringDeviceProfile JDeviceProfile = new JStringDeviceProfile();
                JDeviceProfile.status = (int)serverRes;
                JDeviceProfile.data = DeviceProfileToSend;
                JDeviceProfile.remainProfiles = remainProfiles;

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.DateFormatString = "yyyy-MM-dd";
                string dataResp = JsonConvert.SerializeObject(JDeviceProfile, jsonSettings);
                StandardString info = new StandardString();
                info.data = dataResp;
                this.Publish(publishdata, info);

                //===================================Waiting Response
                int cntTimeOut = 0;
                do
                {
                    if (OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_ADD_PROFILE_SUCCESS ||
                        OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_ADD_PROFILE_UNSUCCESS ||
                        OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_DELETE_PROFILE_SUCCESS ||
                        OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_DELETE_PROFILE_UNSUCCESS ||
                        OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_UPDATE_PROFILE_SUCCESS ||
                        OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_UPDATE_PROFILE_UNSUCCESS)
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }
                while (cntTimeOut++ < 20);

                if (OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_ADD_PROFILE_SUCCESS ||
                    OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_DELETE_PROFILE_SUCCESS ||
                    OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_UPDATE_PROFILE_SUCCESS)
                {
                    OnFlagStatusClient.OnConfirmProfileSuccess = CLIENTCMD.CLIENT_READY;
                    //StatusProfile = STATUSPROFILE.Updated;
                    return true;
                }
                else
                {
                    OnFlagStatusClient.OnConfirmProfileSuccess = CLIENTCMD.CLIENT_READY;
                    //StatusProfile = STATUSPROFILE.Failed;
                    return false;
                }
                //===================================
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public void RequestPersonListImmediately()
        {
            try
            {
                if(!String.IsNullOrEmpty(WebSocketStatus) && WebSocketStatus.Equals(RosStatus.Connected.ToString()))
                {
                    StatusProfile = "Requesting check-in records";
                    dynamic product = new JObject();
                    product.status = SERVERRESPONSE.RESP_REQ_PERSONLIST_IMMEDIATELY;
                    StandardString msg = new StandardString();
                    msg.data = product.ToString();
                    Publish(publishdata, msg);
                }
            }
            catch (Exception ex)
            {
                StatusProfile = "Requesting check-in records failed";
                logFile.Error(ex.Message);
            }
        }

        public bool CheckinServer(string ip, List<CheckinData> person)
        {
            try
            {
                foreach (CheckinData p in person)
                {
                    string[] timeArray = p.TIMECHECK.Split(';');
                    foreach (string tick in timeArray)
                    {
                        long date = long.Parse(tick);
                        DateTime dateTime = new DateTime(date);
                        TimeRecord timeCheck = new TimeRecord(ip, p.SERIAL_ID, dateTime);
                        SqliteDataAccess.InsertDataTimeCheck(timeCheck);
                    }
                }
                StatusProfile = "Check-in records saved";
                return true;
            }
            catch (Exception ex)
            {
                StatusProfile = "Save check-in records failed";
                logFile.Error(ex.Message);
                return false;
            }
            finally
            {
                StatusProfile = "Ready";
            }
        }
        public Byte[] ImageToByte(Image img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Close();

                byteArray = stream.ToArray();
            }
            return byteArray;
        }

        public enum RosStatus
        {
            Pending = 0,
            Connecting = 1,
            Connected = 2,
            Disconnected = 3
        }


        private string _webSocketStatus;
        public String WebSocketStatus
        {
            get
            {
                return _webSocketStatus;
            }
            set
            {
                _webSocketStatus = value;
                OnPropertyChanged("WebSocketStatus");
            }
        }

        private bool _isSendingProfiles;
        public bool IsSendingProfiles
        {
            get
            {
                return _isSendingProfiles;
            }
            set
            {
                _isSendingProfiles = value;
                OnPropertyChanged("IsSendingProfiles");
            }
        }
    }
}
//case CLIENTCMD.REQUEST_PROFILE_ADD:
//                    {
//                        //sendProfile(ip, SERVERRESPONSE.RESP_PROFILE_ADD, new List<Profile>());
//                        //dynamic product=new JOb
//                        break;
//                    }
//                    case CLIENTCMD.REQUEST_PROFILE_UPDATE:
//                    {
//                        //sendProfile(ip, SERVERRESPONSE.RESP_PROFILE_UPDATE, new List<Profile>());
//                        //dynamic product=new JOb
//                        break;
//                    }
//                    case CLIENTCMD.REQUEST_PROFILE_DELETE:
//                    {
//                        //sendProfile(ip, SERVERRESPONSE.RESP_PROFILE_DELETE, new List<Profile>());
//                        //dynamic product=new JOb
//                        break;
//                    }