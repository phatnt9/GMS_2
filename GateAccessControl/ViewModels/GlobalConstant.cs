using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateAccessControl
{
    public class GlobalConstant
    {
        public enum ProfileStatus
        {
            Active = 0,
            Suspended = 1
        }
        public enum ClientStatus
        {
            Synced = 0,
            Unsync = 1
        }
        public enum ServerStatus
        {
            None = 0,
            Add = 1,
            Update = 2,
            Remove = 3,
            Syncing = 4
        }

        public static readonly string AppPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\";
        public static readonly string DatabasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB\";
        public static readonly string ImagePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\";

        public static string messageDuplicated = "{0} is duplicated.";
        public static string messageSaveSucced = "Save operation succeeded.";
        public static string messageSaveFail = "Failed to save. Please try again.";
        public static string messageValidate = "{0} is mandatory. Please enter {1}.";
        public static string messageNothingSelected = "Nothing selected.";
        public static string messageDeleteConfirm = "Do you want to delete the selected {0}?";
        public static string messageDeleteSucced = "Delete operation succeeded.";
        public static string messageDeleteFail = "Failed to delete. Please try again.";
        public static string messageDeleteUse = "Can\'t delete {0} because it has been using on {1}.";
        public static string messageValidateNumber = "{0} must be {1} than {2}.";
        public static string messageNoDataSave = "There is no updated data to save.";

        public static string messageTitileInformation = "Information";
        public static string messageTitileError = "Error";
        public static string messageTitileWarning = "Warning";

        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void CreateFolderToSaveData()
        {
            try
            {
                if (!Directory.Exists(AppPath))
                {
                    Directory.CreateDirectory(AppPath);
                }
                if (!Directory.Exists(ImagePath))
                {
                    Directory.CreateDirectory(ImagePath);
                }
                if (!File.Exists(ImagePath + "default.png"))
                {
                    string DefaultImageFileNamePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\default.png";
                    File.Copy(Environment.CurrentDirectory + @"\Resources\default.png", DefaultImageFileNamePath, true);
                }
                if (!Directory.Exists(DatabasePath))
                {
                    Directory.CreateDirectory(DatabasePath);
                }
                //if (!File.Exists(Environment.GetFolderPath(DatabasePath + "Datastore.db"))
                //{
                //    string DBFileNamePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB\Datastore.db";
                //    //SQLiteConnection.CreateFile(DBFileNamePath);
                //    File.Copy(Environment.CurrentDirectory + @"\Datastore.db", DBFileNamePath, false);
                //}
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }
        public enum Gender
        {
            Male = 0,
            Female = 1
        }
    }
}
