using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GateAccessControl
{
    public class SqliteDataAccess
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Return the connection string to main Database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string LoadConnectionString(string id = "Default")
        {
            //return ConfigurationManager.ConnectionStrings[id].ConnectionString;
            GlobalConstant.CreateFolderToSaveData();
            string test = "Data Source=" + GlobalConstant.DatabasePath + @"\MyDatabase.db;Version=3;";
            //string test = "Data Source=" + GlobalConstant.DatabasePath + @".\Datastore.db;Version=3;";
            return test;
        }

        public static List<CardType> LoadCardTypes()
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    var output = cnn.Query<CardType>("SELECT * FROM DT_CLASS", new DynamicParameters());
                    return output.ToList();
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return new List<CardType>();
            }
        }

        /// <summary>
        /// 0: Load all
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static List<Device> LoadDevices(int deviceId)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    var p = new DynamicParameters();
                    p.Add("@DEVICE_ID", deviceId);
                    if (deviceId == 0)
                    {
                        var output = cnn.Query<Device>("SELECT * FROM DT_DEVICE", p);
                        return output.ToList();
                    }
                    else
                    {
                        var output = cnn.Query<Device>("SELECT * FROM DT_DEVICE WHERE DT_DEVICE.DEVICE_ID = @DEVICE_ID", p);
                        return output.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return new List<Device>();
            }
        }

        public static List<DeviceProfile> LoadDeviceProfiles(int deviceId, string type, string group, string pinno)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    type = (type == null) ? "" : type;
                    group = (group == null) ? "" : group;
                    pinno = (pinno == null) ? "" : pinno;
                    var p = new DynamicParameters();
                    p.Add("@CLASS_NAME", "%" + type + "%");
                    p.Add("@SUB_CLASS", "%" + group + "%");
                    p.Add("@PIN_NO", "%" + pinno + "%");
                    var output = cnn.Query<DeviceProfile>("SELECT * FROM DT_DEVICE_PROFILES_" + deviceId + " WHERE ((CLASS_NAME LIKE (@CLASS_NAME)) AND (SUB_CLASS LIKE (@SUB_CLASS)) AND (PIN_NO LIKE (@PIN_NO)))", p);
                    return output.ToList();
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return new List<DeviceProfile>();
            }
        }

        public static List<Profile> LoadProfiles(string type, string group, string pinno)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    type = (type == null) ? "" : type;
                    group = (group == null) ? "" : group;
                    pinno = (pinno == null) ? "" : pinno;
                    var p = new DynamicParameters();
                    p.Add("@CLASS_NAME", "%" + type + "%");
                    p.Add("@SUB_CLASS", "%" + group + "%");
                    p.Add("@PIN_NO", "%" + pinno + "%");
                    var output = cnn.Query<Profile>("SELECT * FROM DT_PROFILE WHERE ((CLASS_NAME LIKE (@CLASS_NAME)) AND (SUB_CLASS LIKE (@SUB_CLASS)) AND (PIN_NO LIKE (@PIN_NO)))", p);
                    return output.ToList();
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return new List<Profile>();
            }
        }

        public static List<TimeRecord> LoadTimeChecks(string PIN_NO, DateTime time, string ip = null)
        {
            try
            {
                if (ip == null)
                {
                    if (time != DateTime.MinValue)
                    {
                        using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                        {
                            var p = new DynamicParameters();
                            p.Add("@PIN_NO", PIN_NO);
                            p.Add("@FROM", time);
                            p.Add("@TO", time.AddDays(1));

                            var output = cnn.Query<TimeRecord>("SELECT * FROM DT_TIMECHECK INNER JOIN DT_PROFILE,DT_DEVICE ON " +
                                "(DT_PROFILE.PIN_NO = DT_TIMECHECK.PIN_NO AND DT_DEVICE.DEVICE_IP = DT_TIMECHECK.DEVICE_IP)" +
                                " WHERE (TIMECHECK_TIME >= @FROM AND TIMECHECK_TIME < @TO) AND (DT_PROFILE.PIN_NO = @PIN_NO)", p);

                            //var output = cnn.Query<TimeRecord>("SELECT * FROM DT_TIMECHECK" +
                            //    " WHERE (DT_TIMECHECK.PIN_NO = @PIN_NO)", p);
                            return output.ToList();
                        }
                    }
                    else
                    {
                        using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                        {
                            var p = new DynamicParameters();
                            p.Add("@PIN_NO", PIN_NO);

                            var output = cnn.Query<TimeRecord>("SELECT * FROM DT_TIMECHECK INNER JOIN DT_PROFILE,DT_DEVICE ON " +
                                "(DT_PROFILE.PIN_NO = DT_TIMECHECK.PIN_NO AND DT_DEVICE.DEVICE_IP = DT_TIMECHECK.DEVICE_IP)" +
                                " WHERE (DT_PROFILE.PIN_NO = @PIN_NO)", p);

                            //var output = cnn.Query<TimeRecord>("SELECT * FROM DT_TIMECHECK" +
                            //    " WHERE (DT_TIMECHECK.PIN_NO = @PIN_NO)", p);

                            return output.ToList();
                        }
                    }
                }
                else
                {
                    if (time != DateTime.MinValue)
                    {
                        using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                        {
                            var p = new DynamicParameters();
                            p.Add("@PIN_NO", PIN_NO);
                            p.Add("@DEVICE_IP", ip);
                            p.Add("@FROM", time);
                            p.Add("@TO", time.AddDays(1));

                            var output = cnn.Query<TimeRecord>("SELECT * FROM DT_TIMECHECK INNER JOIN DT_PROFILE,DT_DEVICE ON " +
                                "(DT_PROFILE.PIN_NO = DT_TIMECHECK.PIN_NO AND DT_DEVICE.DEVICE_IP = DT_TIMECHECK.DEVICE_IP)" +
                                " WHERE (TIMECHECK_TIME >= @FROM AND TIMECHECK_TIME < @TO) AND (DT_DEVICE.DEVICE_IP = @DEVICE_IP)", p);
                            return output.ToList();
                        }
                    }
                    else
                    {
                        using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                        {
                            var p = new DynamicParameters();
                            p.Add("@PIN_NO", PIN_NO);
                            p.Add("@DEVICE_IP", ip);
                            var output = cnn.Query<TimeRecord>("SELECT * FROM DT_TIMECHECK INNER JOIN DT_PROFILE,DT_DEVICE ON " +
                                "(DT_PROFILE.PIN_NO = DT_TIMECHECK.PIN_NO AND DT_DEVICE.DEVICE_IP = DT_TIMECHECK.DEVICE_IP)" +
                                " WHERE (DT_DEVICE.DEVICE_IP = @DEVICE_IP)", p);
                            return output.ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return new List<TimeRecord>();
            }
        }

        public static bool InsertDevice(Device device)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    var p = new DynamicParameters();
                    p.Add("@DEVICE_IP", device.DEVICE_IP);
                    p.Add("@DEVICE_NAME", device.DEVICE_NAME);
                    p.Add("@DEVICE_STATUS", device.DEVICE_STATUS);
                    p.Add("@DEVICE_NOTE", device.DEVICE_NOTE);
                    cnn.Execute("INSERT INTO DT_DEVICE " +
                        "(DEVICE_IP,DEVICE_NAME,DEVICE_STATUS,DEVICE_NOTE) " +
                        "VALUES " +
                        "(@DEVICE_IP, @DEVICE_NAME, @DEVICE_STATUS,@DEVICE_NOTE)", device);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool InsertProfile(Profile profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("INSERT INTO DT_PROFILE " +
                        "(PIN_NO,AD_NO,PROFILE_NAME,CLASS_NAME,SUB_CLASS,GENDER,DOB,DISU,EMAIL,ADDRESS," +
                        "PHONE,PROFILE_STATUS,IMAGE,DATE_TO_LOCK,CHECK_DATE_TO_LOCK," +
                        "LICENSE_PLATE,LIST_DEVICE_ID,DATE_CREATED,DATE_MODIFIED) " +
                        "VALUES (@PIN_NO,@AD_NO,@PROFILE_NAME,@CLASS_NAME,@SUB_CLASS,@GENDER,@DOB,@DISU,@EMAIL,@ADDRESS," +
                        "@PHONE,@PROFILE_STATUS,@IMAGE,@DATE_TO_LOCK,@CHECK_DATE_TO_LOCK," +
                        "@LICENSE_PLATE,@LIST_DEVICE_ID,@DATE_CREATED,@DATE_MODIFIED)", profile);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool InsertClass(CardType cardType)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("INSERT INTO DT_CLASS (CLASS_NAME) VALUES (@CLASS_NAME)", cardType);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool InsertDeviceProfile(int deviceId, DeviceProfile _profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("INSERT INTO DT_DEVICE_PROFILES_" + deviceId +
                        " (PIN_NO,AD_NO,PROFILE_NAME,CLASS_NAME,SUB_CLASS,GENDER,DOB,DISU,EMAIL,ADDRESS," +
                        "PHONE,PROFILE_STATUS,IMAGE,DATE_TO_LOCK,CHECK_DATE_TO_LOCK," +
                        "LICENSE_PLATE,DATE_CREATED,DATE_MODIFIED,SERVER_STATUS,CLIENT_STATUS,ACTIVE_TIME) " +
                        "VALUES (@PIN_NO,@AD_NO,@PROFILE_NAME,@CLASS_NAME,@SUB_CLASS,@GENDER,@DOB,@DISU,@EMAIL,@ADDRESS," +
                        "@PHONE,@PROFILE_STATUS,@IMAGE,@DATE_TO_LOCK,@CHECK_DATE_TO_LOCK," +
                        "@LICENSE_PLATE,@DATE_CREATED,@DATE_MODIFIED,@SERVER_STATUS,@CLIENT_STATUS,@ACTIVE_TIME)", _profile);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool InsertDeviceProfile(int deviceId, List<Profile> _profiles)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    string insertCmd = "INSERT INTO DT_DEVICE_PROFILES_" + deviceId +
                        " (PIN_NO,AD_NO,PROFILE_NAME,CLASS_NAME,SUB_CLASS,GENDER,DOB,DISU,EMAIL,ADDRESS," +
                        "PHONE,PROFILE_STATUS,IMAGE,DATE_TO_LOCK,CHECK_DATE_TO_LOCK," +
                        "LICENSE_PLATE,DATE_CREATED,DATE_MODIFIED,SERVER_STATUS,CLIENT_STATUS,ACTIVE_TIME) " +
                        "VALUES (@PIN_NO,@AD_NO,@PROFILE_NAME,@CLASS_NAME,@SUB_CLASS,@GENDER,@DOB,@DISU,@EMAIL,@ADDRESS," +
                        "@PHONE,@PROFILE_STATUS,@IMAGE,@DATE_TO_LOCK,@CHECK_DATE_TO_LOCK," +
                        "@LICENSE_PLATE,@DATE_CREATED,@DATE_MODIFIED,@SERVER_STATUS,@CLIENT_STATUS,@ACTIVE_TIME)";

                    foreach (Profile p in _profiles)
                    {
                        DeviceProfile dp = new DeviceProfile(p);
                        cnn.Execute(insertCmd, dp);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool InsertTimeCheck(TimeRecord timeRecord)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("INSERT INTO DT_TIMECHECK (PIN_NO,TIMECHECK_TIME,DEVICE_IP) VALUES (@PIN_NO, @TIMECHECK_TIME, @DEVICE_IP)", timeRecord);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool UpdateDevice(Device device)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("UPDATE DT_DEVICE SET " +
                            "DEVICE_IP = @DEVICE_IP, " +
                            "DEVICE_NAME = @DEVICE_NAME, " +
                            "DEVICE_STATUS = @DEVICE_STATUS, " +
                            "DEVICE_NOTE = @DEVICE_NOTE " +
                            "WHERE DEVICE_ID = @DEVICE_ID", device);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool UpdateProfile(Profile profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("UPDATE DT_PROFILE SET " +
                        "PIN_NO = @PIN_NO, " +
                        "AD_NO = @AD_NO, " +
                        "PROFILE_NAME = @PROFILE_NAME, " +
                        "CLASS_NAME = @CLASS_NAME, " +
                        "SUB_CLASS = @SUB_CLASS, " +
                        "GENDER = @GENDER, " +
                        "DOB = @DOB, " +
                        "DISU = @DISU, " +
                        "EMAIL = @EMAIL, " +
                        "ADDRESS = @ADDRESS, " +
                        "PHONE = @PHONE, " +
                        "PROFILE_STATUS = @PROFILE_STATUS, " +
                        "IMAGE = @IMAGE, " +
                        "DATE_TO_LOCK = @DATE_TO_LOCK, " +
                        "CHECK_DATE_TO_LOCK = @CHECK_DATE_TO_LOCK, " +
                        "LICENSE_PLATE = @LICENSE_PLATE, " +
                        "LIST_DEVICE_ID = @LIST_DEVICE_ID, " +
                        "DATE_CREATED = @DATE_CREATED, " +
                        "DATE_MODIFIED = @DATE_MODIFIED " +
                        "WHERE PIN_NO = @PIN_NO", profile);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool UpdateDeviceProfile(int deviceId, DeviceProfile _profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("UPDATE DT_DEVICE_PROFILES_" + deviceId + " SET " +
                        "PIN_NO = @PIN_NO, " +
                        "AD_NO = @AD_NO, " +
                        "PROFILE_NAME = @PROFILE_NAME, " +
                        "CLASS_NAME = @CLASS_NAME, " +
                        "SUB_CLASS = @SUB_CLASS, " +
                        "GENDER = @GENDER, " +
                        "DOB = @DOB, " +
                        "DISU = @DISU, " +
                        "EMAIL = @EMAIL, " +
                        "ADDRESS = @ADDRESS, " +
                        "PHONE = @PHONE, " +
                        "PROFILE_STATUS = @PROFILE_STATUS, " +
                        "IMAGE = @IMAGE, " +
                        "DATE_TO_LOCK = @DATE_TO_LOCK, " +
                        "CHECK_DATE_TO_LOCK = @CHECK_DATE_TO_LOCK, " +
                        "LICENSE_PLATE = @LICENSE_PLATE, " +
                        "DATE_CREATED = @DATE_CREATED, " +
                        "DATE_MODIFIED = @DATE_MODIFIED, " +
                        "SERVER_STATUS = @SERVER_STATUS, " +
                        "CLIENT_STATUS = @CLIENT_STATUS, " +
                        "ACTIVE_TIME = @ACTIVE_TIME " +
                        "WHERE PIN_NO = @PIN_NO", _profile);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool UpdateCardType(CardType cardType)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("UPDATE DT_CLASS SET " +
                        "CLASS_NAME = @CLASS_NAME " +
                        "WHERE CLASS_ID = @CLASS_ID", cardType);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool DeleteDevice(Device device)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("DELETE FROM DT_DEVICE WHERE DEVICE_ID = @DEVICE_ID", device);
                }
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("DROP TABLE IF EXISTS DT_DEVICE_PROFILES_" + device.DEVICE_ID, device);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool DeleteProfile(Profile profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("DELETE FROM DT_PROFILE WHERE PIN_NO = @PIN_NO", profile);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool DeleteCardType(CardType cardType)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("DELETE FROM DT_CLASS WHERE CLASS_ID = @CLASS_ID", cardType);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool DeleteDeviceProfile(int deviceId, DeviceProfile _profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("DELETE FROM DT_DEVICE_PROFILES_" + deviceId + " WHERE PIN_NO = @PIN_NO", _profile);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Create dynamic table containt profiles for each device in main Database
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool CreateDeviceProfilesTable(string tableName)
        {
            if (File.Exists(GlobalConstant.DatabasePath + @"\MyDatabase.db"))
            {
                try
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        cnn.Execute("CREATE TABLE IF NOT EXISTS \"" + tableName + "\" " +
                               "(\"PROFILE_ID\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, " +
                               "\"PIN_NO\" TEXT NOT NULL UNIQUE, " +
                               "\"AD_NO\" TEXT NOT NULL, " +
                               "\"PROFILE_NAME\" TEXT NOT NULL, " +
                               "\"CLASS_NAME\" TEXT NOT NULL, " +
                               "\"SUB_CLASS\" TEXT, " +
                               "\"GENDER\" TEXT NOT NULL, " +
                               "\"DOB\" DATE NOT NULL, " +
                               "\"DISU\" DATE NOT NULL, " +
                               "\"EMAIL\" TEXT, " +
                               "\"ADDRESS\" TEXT, " +
                               "\"PHONE\" TEXT, " +
                               "\"PROFILE_STATUS\" TEXT, " +
                               "\"IMAGE\" TEXT NOT NULL, " +
                               "\"DATE_TO_LOCK\" DATE, " +
                               "\"CHECK_DATE_TO_LOCK\" INTEGER NOT NULL DEFAULT 0, " +
                               "\"LICENSE_PLATE\" TEXT, " +
                               "\"DATE_CREATED\" DATE, " +
                               "\"DATE_MODIFIED\" DATE, " +
                               "\"SERVER_STATUS\" TEXT, " +
                               "\"CLIENT_STATUS\" TEXT, " +
                               "\"ACTIVE_TIME\" TEXT)");
                    }
                }
                catch (Exception ex)
                {
                    logFile.Error(ex.Message);
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Create main Database with its tables
        /// </summary>
        /// <returns></returns>
        public static bool CreateDatabase()
        {
            if (!File.Exists(GlobalConstant.DatabasePath + @"\MyDatabase.db"))
            {
                try
                {
                    SQLiteConnection.CreateFile(GlobalConstant.DatabasePath + @"\MyDatabase.db");
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        cnn.Execute("CREATE TABLE \"DT_PROFILE\" " +
                            "(\"PROFILE_ID\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, " +
                            "\"PIN_NO\" TEXT NOT NULL UNIQUE, " +
                            "\"AD_NO\" TEXT NOT NULL, " +
                            "\"PROFILE_NAME\" TEXT NOT NULL, " +
                            "\"CLASS_NAME\" TEXT NOT NULL, " +
                            "\"SUB_CLASS\" TEXT, " +
                            "\"GENDER\" TEXT NOT NULL, " +
                            "\"DOB\" DATE NOT NULL, " +
                            "\"DISU\" DATE NOT NULL, " +
                            "\"EMAIL\" TEXT, " +
                            "\"ADDRESS\" TEXT, " +
                            "\"PHONE\" TEXT, " +
                            "\"PROFILE_STATUS\" TEXT NOT NULL, " +
                            "\"IMAGE\" TEXT NOT NULL, " +
                            "\"DATE_TO_LOCK\" DATE, " +
                            "\"CHECK_DATE_TO_LOCK\" INTEGER NOT NULL DEFAULT 0, " +
                            "\"LICENSE_PLATE\" TEXT, " +
                            "\"LIST_DEVICE_ID\" TEXT, " +
                            "\"DATE_CREATED\" DATE, " +
                            "\"DATE_MODIFIED\" DATE)");

                        cnn.Execute("CREATE TABLE \"DT_TIMECHECK\" " +
                            "(\"TIMECHECK_ID\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
                            "\"TIMECHECK_TIME\" DATE NOT NULL," +
                            "\"PIN_NO\" TEXT NOT NULL," +
                            "\"DEVICE_IP\" TEXT NOT NULL)");

                        cnn.Execute("CREATE TABLE \"DT_DEVICE\" " +
                            "(\"DEVICE_ID\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
                            "\"DEVICE_IP\" TEXT NOT NULL UNIQUE," +
                            "\"DEVICE_NAME\" TEXT NOT NULL UNIQUE," +
                            "\"DEVICE_STATUS\" TEXT NOT NULL," +
                            "\"DEVICE_NOTE\" TEXT)");

                        cnn.Execute("CREATE TABLE \"DT_CLASS\" " +
                            "(\"CLASS_ID\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
                            "\"CLASS_NAME\" TEXT NOT NULL UNIQUE)");
                    }
                }
                catch (Exception ex)
                {
                    logFile.Error(ex.Message);
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool InsertDataDeviceProfiles_DUY(string _tableName, DeviceProfile _profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    string txtQuery = "INSERT INTO " + _tableName + "(  PIN_NO, NAME, CLASS, GENDER, DOB, EMAIL, ADDRESS, PHONE, ADNO, " +
                    "IMAGE, DISU, DATE_TO_LOCK, CHECK_DATE_TO_LOCK,  LICENSE_PLATE, STATUS, SYNC_STATUS, " +
                    "ACTIVE_TIME,  DATE_CREATED, DATE_MODIFIED) VALUES ("
                    // + _profile.ID + ","
                    + "'" + _profile.PIN_NO + "'" + ","
                    + "'" + _profile.PROFILE_NAME + "'" + ","
                    + "'" + _profile.CLASS_NAME + "'" + ","
                    + "'" + _profile.GENDER + "'" + ","
                    + "'" + _profile.DOB + "'" + ","
                    + "'" + _profile.EMAIL + "'" + ","
                    + "'" + _profile.ADDRESS + "'" + ","
                    + "'" + _profile.PHONE + "'" + ","
                    + "'" + _profile.AD_NO + "'" + ","

                    + "'" + _profile.IMAGE + "'" + ","
                    + "'" + _profile.DISU + "'" + ","

                    + "'" + _profile.DATE_TO_LOCK + "'" + ","
                    + "'" + _profile.CHECK_DATE_TO_LOCK + "'" + ","
                    + "'" + _profile.LICENSE_PLATE + "'" + ","
                    + "'" + _profile.PROFILE_STATUS + "'" + ","
                    + "'" + _profile.SERVER_STATUS + "'" + ","
                    + "'" + _profile.CLIENT_STATUS + "'" + ","
                    + "'" + _profile.ACTIVE_TIME + "'" + ","
                    + "'" + _profile.DATE_CREATED + "'" + ","
                    + "'" + _profile.DATE_MODIFIED + "'" +
                    ")";

                    cnn.Execute(txtQuery);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static async Task<List<CardType>> LoadCardTypesAsync()
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;
                string url = $@"{serverIp}:{serverPort}/serverschool/load/cardType";

                Task<string> responseTask = client.GetStringAsync(url);
                var responseString = await responseTask;
                List<CardType> listObjects = JsonConvert.DeserializeObject<List<CardType>>(responseString);
                return listObjects;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return new List<CardType>();
            }
        }

        public static async Task<bool> InsertCardTypesAsync(CardType cardType)
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;
                string url = $@"{serverIp}:{serverPort}/serverschool/insert/cardType";

                var values = new Dictionary<string, string>
                {
                { "item1", JsonConvert.SerializeObject(cardType) }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                bool result = false;
                if (bool.TryParse(responseString, out result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeleteCardTypeAsync(CardType cardType)
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;
                string url = $@"{serverIp}:{serverPort}/serverschool/delete/cardType";

                var values = new Dictionary<string, string>
                {
                { "item1", JsonConvert.SerializeObject(cardType) }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                bool result = false;
                if (bool.TryParse(responseString, out result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static async Task<List<Device>> LoadDevicesAsync(int deviceId)
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;
                string url = $@"{serverIp}:{serverPort}/serverschool/load/deviceInf/?deviceId=" + deviceId;

                Task<string> responseTask = client.GetStringAsync(url);
                var responseString = await responseTask;
                List<Device> listObjects = JsonConvert.DeserializeObject<List<Device>>(responseString);
                return listObjects;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return new List<Device>();
            }
        }

        public static async Task<bool> InsertDeviceAsync(Device device)
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;
                string url = $@"{serverIp}:{serverPort}/serverschool/insert/deviceInf";

                var values = new Dictionary<string, string>
                {
                { "item1", JsonConvert.SerializeObject(device) }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                bool result = false;
                if (bool.TryParse(responseString, out result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeleteDeviceAsync(Device device)
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;
                string url = $@"{serverIp}:{serverPort}/serverschool/delete/deviceInf";

                var values = new Dictionary<string, string>
                {
                { "item1", JsonConvert.SerializeObject(device) }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                bool result = false;
                if (bool.TryParse(responseString, out result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static async Task<bool> CreateDeviceProfilesTableAsync(int deviceId)
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;
                string url = $@"{serverIp}:{serverPort}/serverschool/createTable/deviceProfile/?deviceId={deviceId}";

                Task<string> responseTask = client.GetStringAsync(url);
                var responseString = await responseTask;
                bool result = false;
                if (bool.TryParse(responseString, out result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static async Task<List<DeviceProfile>> LoadDeviceProfilesAsync(int deviceId, string type, string group, string pinno)
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;
                string url = $@"{serverIp}:{serverPort}/serverschool/load/deviceProfile/?deviceId={deviceId}&type={type}&group={group}&pinno={pinno}";

                Task<string> responseTask = client.GetStringAsync(url);
                var responseString = await responseTask;
                List<DeviceProfile> listObjects = JsonConvert.DeserializeObject<List<DeviceProfile>>(responseString);
                return listObjects;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return new List<DeviceProfile>();
            }
        }

        public static async Task<bool> InsertDeviceProfileAsync(int deviceId, DeviceProfile _profile)
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;

                string url = $@"{serverIp}:{serverPort}/serverschool/insert/deviceProfile/?deviceId={deviceId}";

                var values = new Dictionary<string, string>
                {
                { "item1", JsonConvert.SerializeObject(_profile) }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                bool result = false;
                if (bool.TryParse(responseString, out result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static async Task<bool> UpdateDeviceProfileAsync(int deviceId, DeviceProfile _profile)
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;

                string url = $@"{serverIp}:{serverPort}/serverschool/update/deviceProfile/?deviceId={deviceId}";

                var values = new Dictionary<string, string>
                {
                { "item1", JsonConvert.SerializeObject(_profile) }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                bool result = false;
                if (bool.TryParse(responseString, out result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeleteDeviceProfileAsync(int deviceId, DeviceProfile _profile)
        {
            try
            {
                string serverIp = Properties.Settings.Default.WebServerAddress;
                string serverPort = Properties.Settings.Default.WebServerPort;
                string url = $@"{serverIp}:{serverPort}/serverschool/delete/deviceProfile/?deviceId={deviceId}& pinno={_profile.PIN_NO}";

                Task<string> responseTask = client.GetStringAsync(url);
                var responseString = await responseTask;
                bool result = false;
                if (bool.TryParse(responseString, out result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }
    }
}