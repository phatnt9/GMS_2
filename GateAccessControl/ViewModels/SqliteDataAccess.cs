using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateAccessControl
{
    public class SqliteDataAccess
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
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

        public static List<CardType> LoadAllCardType()
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
                return null;
            }
        }
        public static List<Device> LoadAllDevices()
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    var output = cnn.Query<Device>("SELECT * FROM DT_DEVICE", new DynamicParameters());
                    return output.ToList();
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return null;
            }
        }
        public static List<DeviceProfiles> LoadAllDeviceProfiles(Device device, string className = "", string subClass = "")
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    var p = new DynamicParameters();
                    p.Add("@CLASS_NAME", "%" + className + "%");
                    p.Add("@SUB_CLASS", "%" + subClass + "%");
                    var output = cnn.Query<DeviceProfiles>("SELECT * FROM DT_DEVICE_PROFILES_" + device.DEVICE_ID + " WHERE ((CLASS_NAME LIKE (@CLASS_NAME)) AND (SUB_CLASS LIKE (@SUB_CLASS)))", p);
                    return output.ToList();
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return null;
            }
        }
        public static List<Profile> LoadAllProfiles(string className = "", string subClass = "")
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    var p = new DynamicParameters();
                    p.Add("@CLASS_NAME", "%" + className + "%");
                    p.Add("@SUB_CLASS", "%" + subClass + "%");

                    var output = cnn.Query<Profile>("SELECT * FROM DT_PROFILE WHERE ((CLASS_NAME LIKE (@CLASS_NAME)) AND (SUB_CLASS LIKE (@SUB_CLASS)))", p);
                    return output.ToList();
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return null;
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
                return null;
            }
        }
        public static bool InsertDataDevice(Device device)
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
        public static bool InsertDataProfile(Profile profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("INSERT INTO DT_PROFILE " +
                        "(PIN_NO,AD_NO,PROFILE_NAME,CLASS_NAME,SUB_CLASS,GENDER,DOB,DISU,EMAIL,ADDRESS," +
                        "PHONE,PROFILE_STATUS,IMAGE,LOCK_DATE,DATE_TO_LOCK,CHECK_DATE_TO_LOCK," +
                        "LICENSE_PLATE,LIST_DEVICE_ID,DATE_CREATED,DATE_MODIFIED) " +
                        "VALUES (@PIN_NO,@AD_NO,@PROFILE_NAME,@CLASS_NAME,@SUB_CLASS,@GENDER,@DOB,@DISU,@EMAIL,@ADDRESS," +
                        "@PHONE,@PROFILE_STATUS,@IMAGE,@LOCK_DATE,@DATE_TO_LOCK,@CHECK_DATE_TO_LOCK," +
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
        public static bool InsertDataClass(CardType cardType)
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
        public static bool InsertDataDeviceProfiles(string _tableName, DeviceProfiles _profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("INSERT INTO " + _tableName +
                        " (PIN_NO,AD_NO,PROFILE_NAME,CLASS_NAME,SUB_CLASS,GENDER,DOB,DISU,EMAIL,ADDRESS," +
                        "PHONE,PROFILE_STATUS,IMAGE,LOCK_DATE,DATE_TO_LOCK,CHECK_DATE_TO_LOCK," +
                        "LICENSE_PLATE,DATE_CREATED,DATE_MODIFIED,SERVER_STATUS,CLIENT_STATUS,ACTIVE_TIME) " +
                        "VALUES (@PIN_NO,@AD_NO,@PROFILE_NAME,@CLASS_NAME,@SUB_CLASS,@GENDER,@DOB,@DISU,@EMAIL,@ADDRESS," +
                        "@PHONE,@PROFILE_STATUS,@IMAGE,@LOCK_DATE,@DATE_TO_LOCK,@CHECK_DATE_TO_LOCK," +
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
        public static bool InsertDataTimeCheck(TimeRecord timeRecord)
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
        public static bool UpdateDataDevice(Device device)
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
        public static bool UpdateDataProfile(Profile profile)
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
                        "LOCK_DATE = @LOCK_DATE, " +
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

        public static bool UpdateDataDeviceProfiles(string _tableName, DeviceProfiles _profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("UPDATE " + _tableName + " SET " +
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
                        "LOCK_DATE = @LOCK_DATE, " +
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

        public static bool UpdateDataClass(CardType cardType)
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

        public static bool DeleteDataDevice(Device device)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("DELETE FROM DT_DEVICE WHERE DEVICE_ID = @DEVICE_ID", device);
                }
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("DROP TABLE IF EXISTS DT_DEVICE_PROFILES_"+device.DEVICE_ID, device);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }
        public static bool DeleteDataProfile(Profile profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("DELETE FROM DT_PROFILE WHERE PROFILE_ID = @PROFILE_ID", profile);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }
        public static bool DeleteDataCardType(CardType cardType)
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
        public static bool DeleteDataDeviceCardType(DeviceCardType deviceCardType)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("DELETE FROM DT_DEVICE_CLASS WHERE DEVICE_CLASS_ID = @DEVICE_CLASS_ID", deviceCardType);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public static bool DeleteDataDeviceProfiles(string _tableName, DeviceProfiles _profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("DELETE FROM "+ _tableName + " WHERE PROFILE_ID = @PROFILE_ID", _profile);
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
                        cnn.Execute("CREATE TABLE IF NOT EXISTS \""+ tableName + "\" " +
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
                               "\"LOCK_DATE\" DATE, " +
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
                            "\"PROFILE_STATUS\" TEXT, " +
                            "\"IMAGE\" TEXT NOT NULL, " +
                            "\"LOCK_DATE\" DATE, " +
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
                            "\"DEVICE_NAME\" TEXT NOT NULL," +
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
        public static bool InsertDataDeviceProfiles_DUY(string _tableName, DeviceProfiles _profile)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    string txtQuery = "INSERT INTO " + _tableName + "(  PIN_NO, NAME, CLASS, GENDER, DOB, EMAIL, ADDRESS, PHONE, ADNO, " +
                    "IMAGE, DISU, LOCK_DATE, DATE_TO_LOCK, CHECK_DATE_TO_LOCK,  LICENSE_PLATE, STATUS, SYNC_STATUS, " +
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

                    + "'" + _profile.LOCK_DATE + "'" + ","
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
    }
}
