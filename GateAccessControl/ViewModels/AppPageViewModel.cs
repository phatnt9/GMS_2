using GateAccessControl.Views;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace GateAccessControl
{
    public enum AppStatus
    {
        Ready,
        Exporting,
        Searching,
        Completed,
        Finished,
        Cancelled,
        Error,
    }
    public class AppPageViewModel : ViewModelBase
    {
        public ICommand AddDeviceCommand { get; set; }
        public ICommand EditDeviceCommand { get; set; }
        public ICommand RemoveDeviceCommand { get; set; }
        public ICommand SelectDeviceCommand { get; set; }
        public ICommand ConnectDeviceCommand { get; set; }
        public ICommand DisconnectDeviceCommand { get; set; }
        public ICommand RequesTimeRecordCommand { get; set; }

        public ICommand AddProfileCommand { get; set; }
        public ICommand EditProfileCommand { get; set; }
        public ICommand RemoveProfileCommand { get; set; }
        public ICommand SelectProfileCommand { get; set; }
        public ICommand ExportProfilesCommand { get; set; }
        public ICommand StopExportProfilesCommand { get; set; }

        public ICommand DeviceProfilesManageCommand { get; set; }
        public ICommand SelectDeviceProfilesCommand { get; set; }
        
        public ICommand ImportProfilesCommand { get; set; }
        public ICommand ManageClassCommand { get; set; }

        public ICommand SearchClassProfilesCommand { get; set; }
        public ICommand SearchGroupProfilesCommand { get; set; }
        public ICommand SearchOthersProfilesCommand { get; set; }

        public ICommand SearchClassDeviceProfilesCommand { get; set; }
        public ICommand SearchGroupDeviceProfilesCommand { get; set; }
        public ICommand SearchOthersDeviceProfilesCommand { get; set; }

        public ICommand SyncCommand { get; set; }
        public ICommand StopSyncCommand { get; set; }
        public ICommand ReplaceProfileImageCommand { get; set; }

        public ICommand SetTimeDeviceProfileCommnad { get; set; }
        public ICommand SelectedDateChangeCommand { get; set; }
        public ICommand SelectPreviousDateCommand { get; set; }
        public ICommand SelectNextDateCommand { get; set; }

        

        public AppPageViewModel()
        {
            SelectedTimeCheckDate = DateTime.Now;
            ReloadDataDevices();
            ReloadDataProfiles();
            ReloadDataCardTypes();
            SyncProgressValue = 0;
            CreateCheckSuspendProfilesTimer();
            CreateRequestTimeChecksTimer();

            SelectNextDateCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     if (p != null && SelectedTimeCheckDate != null)
                         return true;
                     else
                         return false;
                 },
                 (p) =>
                 {
                     SelectedTimeCheckDate = SelectedTimeCheckDate.AddDays(1);
                 });

            SelectPreviousDateCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     if (p != null && SelectedTimeCheckDate != null)
                         return true;
                     else
                         return false;
                 },
                 (p) =>
                 {
                     SelectedTimeCheckDate = SelectedTimeCheckDate.AddDays(-1);
                 });

            SelectedDateChangeCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     if (p != null && SelectedTimeCheckDate != null)
                         return true;
                     else
                         return false;
                 },
                 (p) =>
                 {
                     ReloadDataTimeCheck(p, SelectedTimeCheckDate);
                 });

            RequesTimeRecordCommand = new RelayCommand<Device>(
                 (p) =>
                 {
                     if(p!= null && p.DeviceItem.WebSocketStatus == "Connected" && p.DeviceItem.IsSendingProfiles == false)
                     {
                         return true;
                     }
                     else
                     {
                         return false;
                     }
                 },
                 (p) =>
                 {
                     try
                     {
                         p.DeviceItem.RequestPersonListImmediately();
                     }
                     catch (Exception ex)
                     {
                         logFile.Error(ex.Message);
                     }
                 });

            StopExportProfilesCommand = new RelayCommand<Device>(
                 (p) =>
                 {
                     //Can Stop when sending Profiles
                     return (IsExportingProfiles) ? true : false;
                 },
                 (p) =>
                 {
                     ExportWorker.CancelAsync();
                 });


            ExportProfilesCommand = new RelayCommand<List<Profile>>(
                 (p) =>
                 {
                     if (p != null && p.Count > 0)
                     {
                         return true;
                     }
                     else
                     {
                         return false;
                     }
                 },
                 (p) =>
                 {
                     ExportProfiles(p);
                 });

            SelectDeviceProfilesCommand = new RelayCommand<DeviceProfiles>(
                 (p) =>
                 {
                         return true;
                 },
                 (p) =>
                 {
                     ParseActiveTimeDeviceProfile_new(p);
                 });


            SetTimeDeviceProfileCommnad = new RelayCommand<List<DeviceProfiles>>(
                 (p) =>
                 {
                     if (p != null && p.Count > 0)
                     {
                         return (CheckNoDeviceIsSyncing() == 0) ? true : false;
                     }
                     else
                     {
                         return false;
                     }
                 },
                 (p) =>
                 {
                     SetTimeDeviceProfile(p);
                     string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                     string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                     ReloadDataProfiles(classSearch, groupSearch);
                 });

            AddProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return true;
                 },
                 (p) =>
                 {
                     AddProfile(p);
                     string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                     string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                     ReloadDataProfiles(classSearch, groupSearch);
                 });

            EditProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     //Cannot when any device is syncing
                     //Console.WriteLine("_numberOfSyncingDevices: "+ CheckNoDeviceIsSyncing());
                     return (p != null && CheckNoDeviceIsSyncing() == 0) ? true : false;
                 },
                 (p) =>
                 {
                     EditProfile(p);
                     string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                     string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                     ReloadDataProfiles(classSearch, groupSearch);
                     if(SelectedDevice != null)
                     {
                         ReloadDataDeviceProfiles(SelectedDevice);
                     }
                 });

            RemoveProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     //Cannot when any device is syncing
                     return (p != null && CheckNoDeviceIsSyncing() == 0 && String.IsNullOrEmpty(p.LIST_DEVICE_ID)) ? true : false;
                 },
                 (p) =>
                 {
                     RemoveProfile(p);
                     string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                     string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                     ReloadDataProfiles(classSearch, groupSearch);
                 });

            SelectProfileCommand = new RelayCommand<List<TimeRecord>>(
                 (p) =>
                 {
                     return true;
                 },
                 (p) =>
                 {
                     ReloadDataTimeCheck(p);
                 });

            ReplaceProfileImageCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return (p != null) ? true : false;
                 },
                 (p) =>
                 {
                     ReplaceProfileImage(p.IMAGE, p);
                     ReloadDataDeviceProfiles(SelectedDevice, (Search_deviceProfiles_class == "All" ? "" : Search_deviceProfiles_class), (Search_deviceProfiles_group == "All" ? "" : Search_deviceProfiles_group));
                 });

            StopSyncCommand = new RelayCommand<Device>(
                 (p) =>
                 {
                     //Can Stop when sending Profiles
                     return (p!= null && p.DeviceItem.IsSendingProfiles) ? true : false;
                 },
                 (p) =>
                 {
                     p.SyncWorker.CancelAsync();
                 });

            SyncCommand = new RelayCommand<List<DeviceProfiles>>(
                 (p) =>
                 {
                     /* 
                      * User must select a Device and some Profiles to be able to Sync Profiles.
                      * Device must be connected before Syncing.
                      * If that Device is in process of Sending something, then you cannot Sync too.
                      * Get list profiles that can be synced depend on its status and server Status
                      */
                     if (SelectedDevice != null && p != null)
                     {
                         if (SelectedDevice.DeviceItem.IsSendingProfiles || !CheckDeviceStatus(SelectedDevice).Equals("Connected"))
                         {
                             return false;
                         } //right
                         //if (SelectedDevice.DeviceItem.IsSendingProfiles)
                         //{
                         //    return false;
                         //} //wrong
                         return (GetCanSyncDeviceProfiles(p).Count > 0) ? true : false;
                     }
                     else
                     {
                         return false;
                     }
                 },
                (p) =>
                {
                    //Sync it
                    SelectedDevice.SyncDeviceProfiles(GetCanSyncDeviceProfiles(p));
                });

            AddDeviceCommand = new RelayCommand<Device>(
                 (p) => true,
                 (p) =>
                 {
                     AddDevice();
                     ReloadDataDevices();
                 });

            EditDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return CanEditOrRemoveDevice(p);
                },
                (p) =>
                {
                    EditDevice(p);
                    ReloadDataDevices();
                });

            RemoveDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return CanEditOrRemoveDevice(p);
                },
                (p) =>
                {
                    if (RemoveDevice(p))
                    {
                        ReloadDataDevices(p);
                    }
                });

            SelectDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return true;
                },
                (p) =>
                {
                    SqliteDataAccess.CreateDeviceProfilesTable("DT_DEVICE_PROFILES_" + p.DEVICE_ID);
                    ReloadDataDeviceProfiles(p);
                });

            DeviceProfilesManageCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return (p != null && !p.DeviceItem.IsSendingProfiles) ? true : false;
                },
                (p) =>
                {
                    ManageDeviceProfiles(p);
                    ReloadDataDeviceProfiles(p);
                    string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                    string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                    ReloadDataProfiles(classSearch, groupSearch);
                });

            ConnectDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return CheckIfDeviceCanConnect(p);
                },
                (p) =>
                {
                    ConnectDevice(p);
                });

            DisconnectDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    if (p == null || p.DeviceItem.IsSendingProfiles)
                    {
                        return false;
                    }
                    else
                    {
                        return !CheckIfDeviceCanConnect(p);
                    }
                },
                (p) =>
                {
                    DisconnectDevice(p);
                });

            SearchOthersProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    SearchProfiles(p);
                });

            SearchOthersDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    SearchDeviceProfiles(p);
                });

            SearchClassProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                    string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                    ReloadDataProfiles(classSearch, groupSearch);
                });

            SearchGroupProfilesCommand = new RelayCommand<ItemCollection>(
                 (p) => true,
                 (p) =>
                 {
                     string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                     string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                     ReloadDataProfiles(classSearch, groupSearch);
                 });

            SearchClassDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDataDeviceProfiles(SelectedDevice, (Search_deviceProfiles_class == "All" ? "" : Search_deviceProfiles_class), (Search_deviceProfiles_group == "All" ? "" : Search_deviceProfiles_group));
                });

            

            SearchGroupDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDataDeviceProfiles(SelectedDevice, (Search_deviceProfiles_class == "All" ? "" : Search_deviceProfiles_class), (Search_deviceProfiles_group == "All" ? "" : Search_deviceProfiles_group));
                });

            ImportProfilesCommand = new RelayCommand<Profile>(
                (p) => true,
                (p) =>
                {
                    ReloadDataCardTypes();
                    IEnumerable<CardType> obsCollection = (IEnumerable<CardType>)Classes;
                    List<CardType> list = new List<CardType>(obsCollection);
                    ImportProfiles(list);
                    string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                    string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                    ReloadDataProfiles(classSearch, groupSearch);
                    ReloadDataCardTypes();
                });

            ManageClassCommand = new RelayCommand<CardType>(
                (p) => true,
                (p) =>
                {
                    ManageClass();
                    ReloadDataCardTypes();
                });
        }

        public string CheckDeviceStatus(Device device)
        {
            if (device.DeviceItem.webSocket != null)
            {
                if (device.DeviceItem.webSocket.IsAlive)
                {
                    return "Connected";
                }
                else
                {
                    return "Connecting";
                }
            }
            else
            {
                return "Disconnected";
            }
        }

        private void ExportProfiles(List<Profile> p)
        {
            ExportWorker = new BackgroundWorker();
            ExportWorker.WorkerSupportsCancellation = true;
            ExportWorker.WorkerReportsProgress = true;
            ExportWorker.DoWork += ExportWorker_DoWork;
            ExportWorker.RunWorkerCompleted += ExportWorker_RunWorkerCompleted;
            ExportWorker.ProgressChanged += ExportWorker_ProgressChanged;

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            saveDialog.FilterIndex = 2;
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<object> arguments = new List<object>();
                arguments.Add(saveDialog);
                arguments.Add(p);
                ExportWorker.RunWorkerAsync(argument: arguments);
            }
        }

        private void ExportWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ExportProfilesProgressValue = e.ProgressPercentage;
        }

        private void ExportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
            ExportProfilesProgressValue = 0;
            ProfilesWorkStatus = AppStatus.Finished.ToString();
            ProfilesWorkStatus = AppStatus.Ready.ToString();
            IsExportingProfiles = false;
        }

        private void ExportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsExportingProfiles = true;
            ProfilesWorkStatus = AppStatus.Exporting.ToString();
            List<object> genericList = e.Argument as List<object>;
            Excel.Application excel = new Excel.Application();
            Excel.Workbook workbook = excel.Workbooks.Add(Type.Missing);
            Excel.Worksheet worksheet = null;
            try
            {
                List<CardType> listClasses = SqliteDataAccess.LoadAllCardType();

                SaveFileDialog saveDialog = genericList[0] as SaveFileDialog;

                excel.DisplayAlerts = false;
                worksheet = workbook.ActiveSheet;

                worksheet.Cells[1, 1] = "No";
                worksheet.Cells[1, 2] = "Name";
                worksheet.Cells[1, 3] = "Adno";
                worksheet.Cells[1, 4] = "Gender";
                worksheet.Cells[1, 5] = "DOB";
                worksheet.Cells[1, 6] = "Disu";
                worksheet.Cells[1, 7] = "Image";
                worksheet.Cells[1, 8] = "PIN No.";
                worksheet.Cells[1, 9] = "Class";
                worksheet.Cells[1, 10] = "Group";//
                worksheet.Cells[1, 11] = "Email";
                worksheet.Cells[1, 12] = "Address";
                worksheet.Cells[1, 13] = "Phone";
                worksheet.Cells[1, 14] = "Status";
                //worksheet.Cells[1, 15] = "Suspended Date";
                worksheet.Cells[1, 15] = "Expire Date";
                worksheet.Cells[1, 16] = "Automatic Suspension";
                worksheet.Cells[1, 17] = "License plate";//
                worksheet.Cells[1, 18] = "Date created";//
                worksheet.Cells[1, 19] = "Date modified";//

                int cellRowIndex = 2;
                int cellColumnIndex = 1;

                List<Profile> profileList = genericList[1] as List<Profile>;

                for (int i = 0; i < profileList.Count; i++)
                {
                    for (int j = 0; j < 19; j++)
                    {
                        if (j == 0)//No
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = i + 1;  }
                        if (j == 1)//Name
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].PROFILE_NAME;  }
                        if (j == 2)//Adno
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].AD_NO;
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].ADNO;
                        }
                        if (j == 3)//Gender
                        {
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].GENDER.ToString();

                            var list = new System.Collections.Generic.List<string>();
                            list.Add("Male");
                            list.Add("Female");
                            var flatList = string.Join(",", list.ToArray());

                            var cell = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cell.Validation.Delete();
                            cell.Validation.Add(
                               Excel.XlDVType.xlValidateList,
                               Excel.XlDVAlertStyle.xlValidAlertInformation,
                               Excel.XlFormatConditionOperator.xlBetween,
                               flatList,
                               Type.Missing);
                            cell.Value2 = profileList[i].GENDER.ToString();
                            cell.Locked = true;
                            cell.Validation.IgnoreBlank = true;
                            cell.Validation.InCellDropdown = true;
                        }
                        if (j == 4)//DOB
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].DOB;}
                        if (j == 5)//Disu
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].DISU;}
                        if (j == 6)//Image
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].IMAGE;
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].IMAGE;
                        }
                        if (j == 7)//PIN No.
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].PIN_NO;
                            //worksheet.Range[cellColumnIndex].NumberFormat = "0";
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].PIN_NO;
                        }

                        if (j == 8)//Class
                        {
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].CLASS;

                            var list = new List<string>();
                            foreach (CardType classes in listClasses)
                            {
                                list.Add(classes.CLASS_NAME);
                            }
                            var flatList = string.Join(",", list.ToArray());

                            var cell = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cell.Validation.Delete();
                            cell.Validation.Add(
                               Excel.XlDVType.xlValidateList,
                               Excel.XlDVAlertStyle.xlValidAlertInformation,
                               Excel.XlFormatConditionOperator.xlBetween,
                               flatList,
                               Type.Missing);
                            cell.Value2 = profileList[i].CLASS_NAME;
                            cell.Validation.IgnoreBlank = true;
                            cell.Validation.InCellDropdown = true;
                        }

                        if (j == 9)//Class
                        {
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].CLASS;

                            var list = new System.Collections.Generic.List<string>();
                            list.Add("Group 1");
                            list.Add("Group 2");
                            list.Add("Group 3");
                            list.Add("Group 4");
                            list.Add("Group 5");
                            list.Add("Group 6");
                            list.Add("Group 7");
                            list.Add("Group 8");
                            list.Add("Group 9");
                            list.Add("Group 10");
                            var flatList = string.Join(",", list.ToArray());

                            var cell = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cell.Validation.Delete();
                            cell.Validation.Add(
                               Excel.XlDVType.xlValidateList,
                               Excel.XlDVAlertStyle.xlValidAlertInformation,
                               Excel.XlFormatConditionOperator.xlBetween,
                               flatList,
                               Type.Missing);
                            cell.Value2 = profileList[i].SUB_CLASS;
                            cell.Validation.IgnoreBlank = true;
                            cell.Validation.InCellDropdown = true;
                        }

                        if (j == 10)//Email
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].EMAIL; }
                        if (j == 11)//Address
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].ADDRESS;}
                        if (j == 12)//Phone
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].PHONE;
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].PHONE;
                        }
                        if (j == 13)//Status
                        {
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].STATUS;

                            var list = new System.Collections.Generic.List<string>();
                            list.Add("Active");
                            list.Add("Suspended");
                            var flatList = string.Join(",", list.ToArray());

                            var cell = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cell.Validation.Delete();
                            cell.Validation.Add(
                               Excel.XlDVType.xlValidateList,
                               Excel.XlDVAlertStyle.xlValidAlertInformation,
                               Excel.XlFormatConditionOperator.xlBetween,
                               flatList,
                               Type.Missing);
                            cell.Value2 = profileList[i].PROFILE_STATUS;
                            cell.Locked = true;
                            cell.Validation.IgnoreBlank = true;
                            cell.Validation.InCellDropdown = true;
                        }

                        if (j == 14)//Expire Date
                        {
                            if (profileList[i].CHECK_DATE_TO_LOCK == true)
                            {
                                { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].DATE_TO_LOCK; }
                            }
                            else
                            {
                                { worksheet.Cells[cellRowIndex, cellColumnIndex] = ""; }
                            }
                        }
                        if (j == 15)// Automatic Suspension
                        {
                            var list = new System.Collections.Generic.List<string>();
                            list.Add("TRUE");
                            list.Add("FALSE");
                            var flatList = string.Join(",", list.ToArray());

                            var cell = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cell.Validation.Delete();
                            cell.Validation.Add(
                               Excel.XlDVType.xlValidateList,
                               Excel.XlDVAlertStyle.xlValidAlertInformation,
                               Excel.XlFormatConditionOperator.xlBetween,
                               flatList,
                               Type.Missing);
                            cell.Value2 = profileList[i].CHECK_DATE_TO_LOCK;
                            cell.Locked = true;
                            cell.Validation.IgnoreBlank = true;
                            cell.Validation.InCellDropdown = true;
                        }
                        if (j == 16)//License plate
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].LICENSE_PLATE; }
                        if (j == 17)//Date created
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].DATE_CREATED; }
                        if (j == 18)//Date modified
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].DATE_MODIFIED; }

                        cellColumnIndex++;
                    }
                    cellColumnIndex = 1;
                    cellRowIndex++;
                    if (ExportWorker.CancellationPending)
                    {
                        break;
                    }
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profileList.Count);
                }

                worksheet.Columns.AutoFit();
                workbook.SaveAs(saveDialog.FileName, Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing, false, false, Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing);
                MessageBox.Show("Export Successful");
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
            finally
            {
                excel.Quit();
                workbook = null;
                excel = null;
            }
        }

        private void SetTimeDeviceProfile(List<DeviceProfiles> p)
        {
            string myActiveTime = GetActiveTimeFromTextBoxes();
            if(!String.IsNullOrEmpty(myActiveTime))
            {
                foreach (DeviceProfiles item in p)
                {
                    item.ACTIVE_TIME = myActiveTime;
                    if(item.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString() && 
                        item.SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString())
                    {
                        item.SERVER_STATUS = GlobalConstant.ServerStatus.Update.ToString();
                    }
                    if(SqliteDataAccess.UpdateDataDeviceProfiles(SelectedDevice.DEVICE_ID, item))
                    {
                        ApplyActiveTimeStatus = "Success!";
                    }
                    else
                    {

                        ApplyActiveTimeStatus = "Unsuccess!";
                    }
                }
            }
           
        }

        private string GetActiveTimeFromTextBoxes()
        {
            string returnStr = "";
            int count = 0;
            if(ActiveTimeSchedule_1)
            {
                if (ValidateTime(Active_from_1))
                {
                    returnStr += Active_from_1;
                    count++;
                }
                returnStr += "-";
                if (ValidateTime(Active_to_1))
                {
                    returnStr += Active_to_1;
                    count++;
                }
                returnStr += "-";
                if (ValidateTime(Active_from_2))
                {
                    returnStr += Active_from_2;
                    count++;
                }
                returnStr += "-";
                if (ValidateTime(Active_to_2))
                {
                    returnStr += Active_to_2;
                    count++;
                }
                if (count == 4)
                {
                    return returnStr;
                }
                else
                {
                    ApplyActiveTimeStatus = "Wrong time format! (HH:mm)";
                    return null;
                }
            }
            if (ActiveTimeSchedule_2)
            {
                returnStr = "00:00-23:59-00:00-23:59";
                return returnStr;
            }
            ApplyActiveTimeStatus = "Please selected a Schedule!";
            return null;
        }

        private void ParseActiveTimeDeviceProfile_new(DeviceProfiles p)
        {
            if (p!= null && !String.IsNullOrEmpty(p.ACTIVE_TIME))
            {
                string[] listVar = p.ACTIVE_TIME.Split('-');
                for (int i = 0; i < 4; i++)
                {
                    if (i == 0 && ValidateTime(listVar[i]))
                    {
                        Active_from_1 = listVar[i];
                    }
                    if (i == 1 && ValidateTime(listVar[i]))
                    {
                        Active_to_1 = listVar[i];
                    }
                    if (i == 2 && ValidateTime(listVar[i]))
                    {
                        Active_from_2 = listVar[i];
                    }
                    if (i == 3 && ValidateTime(listVar[i]))
                    {
                        Active_to_2 = listVar[i];
                    }
                }
            }
        }

        private void ParseActiveTimeDeviceProfile_old(DeviceProfiles p)
        {
            if (!String.IsNullOrEmpty(p.ACTIVE_TIME))
            {
                string[] listVar = p.ACTIVE_TIME.Split(';');
                for (int i=0 ; i<2 ; i++)
                {
                    if (i == 0)
                    {
                        string[] listTime = listVar[i].Split(',');
                        for (int y = 0; y < 2; y++)
                        {
                            if (y == 0)
                            {
                                if (ValidateTime(listTime[y]))
                                {
                                    Active_from_1 = listTime[y];
                                }
                            }
                            if (y == 1)
                            {
                                if (ValidateTime(listTime[y]))
                                {
                                    Active_to_1 = listTime[y];
                                }
                            }
                        }
                    }
                    if (i == 1)
                    {
                        string[] listTime = listVar[i].Split(',');
                        for (int y = 0; y < 2; y++)
                        {
                            if (y == 0)
                            {
                                if (ValidateTime(listTime[y]))
                                {
                                    Active_from_2 = listTime[y];
                                }
                            }
                            if (y == 1)
                            {
                                if (ValidateTime(listTime[y]))
                                {
                                    Active_to_2 = listTime[y];
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool ValidateTime(string time)
        {
            DateTime ignored;
            return DateTime.TryParseExact(time, "HH:mm",
                                          CultureInfo.InvariantCulture,
                                          DateTimeStyles.None,
                                          out ignored);
        }

        private void CreateRequestTimeChecksTimer()
        {
            System.Timers.Timer RequestTimeChecksTimer = new System.Timers.Timer(); //One second, (use less to add precision, use more to consume less processor time
            RequestTimeChecksTimer.Interval = Properties.Settings.Default.RequestTimeCheckInterval;
            RequestTimeChecksTimer.Elapsed += RequestTimeChecksTimer_Elapsed;
            RequestTimeChecksTimer.AutoReset = true;
            RequestTimeChecksTimer.Start();
        }

        private void RequestTimeChecksTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("RequestTimeChecksTimer_Elapsed");
            if (CheckNoDeviceIsSyncing() == 0)
            {
                Task.Run(() =>
                {
                    try
                    {
                        foreach (Device device in Devices)
                        {
                            device.DeviceItem.RequestPersonListImmediately();
                        }
                    }
                    catch (Exception ex)
                    {
                        logFile.Error(ex.Message);
                    }
                });
            }
        }
        
        private void CreateCheckSuspendProfilesTimer()
        {
            System.Timers.Timer SuspendStudentCheckTimer = new System.Timers.Timer(30000); //One second, (use less to add precision, use more to consume less processor time
            lastHour = DateTime.Now.Hour;
            lastSec = DateTime.Now.Second;
            SuspendStudentCheckTimer.Elapsed += SuspendStudentCheckTimer_Elapsed;
            SuspendStudentCheckTimer.Start();
        }
        private void SuspendStudentCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(CheckNoDeviceIsSyncing() == 0)
            {
                if (lastHour < DateTime.Now.Hour || (lastHour == 23 && DateTime.Now.Hour == 0))
                {
                    lastHour = DateTime.Now.Hour;
                    Console.WriteLine("Catch " + DateTime.Now.ToLongTimeString() + "s.");
                    CheckSuspendAllProfile();
                }
            }
        }

        public void CheckSuspendAllProfile()
        {
            try
            {
                //Get all Profile
                List<Profile> profiles = SqliteDataAccess.LoadAllProfiles();

                //Check status --> check date to Suspend --> Suspend(active)
                foreach (Profile profile in profiles)
                {
                    if (profile.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Active.ToString()) && 
                        profile.CHECK_DATE_TO_LOCK == true)
                    {
                        if (DateTime.Now > profile.DATE_TO_LOCK)
                        {
                            profile.CHECK_DATE_TO_LOCK = false;
                            profile.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                            profile.DATE_MODIFIED = DateTime.Now;
                            if (SqliteDataAccess.UpdateDataProfile(profile))
                            {
                                //MessageBox.Show("Profile saved!");
                                UpdateProfileToAllDevice(profile);
                            }
                            else
                            {
                                //MessageBox.Show("Field with (*) is mandatory!");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
            finally
            {
                string classSearch = Search_profiles_class == "All" ? "" : Search_profiles_class;
                string groupSearch = Search_profiles_group == "All" ? "" : Search_profiles_group;
                ReloadDataProfiles(classSearch, groupSearch);
            }
        }
        
        private void RemoveProfile(Profile p)
        {
            if (String.IsNullOrEmpty(p.LIST_DEVICE_ID))
            {
                SqliteDataAccess.DeleteDataProfile(p);
            }
        }

        private void AddProfile(Profile p)
        {
            AddProfileWindow addProfileWindow = new AddProfileWindow();
            addProfileWindow.ShowDialog();
        }

        private void EditProfile(Profile p)
        {
            EditProfileWindow editProfileWindow = new EditProfileWindow(p);
            editProfileWindow.ShowDialog();
        }

        private void ReplaceProfileImage(string origin, Profile p)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Title = "Browse JPEG Image",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "JPEG",
                Filter = "All JPEG Files (*.jpg)|*.jpg",
                FilterIndex = 1,
                RestoreDirectory = true
                //ReadOnlyChecked = true,
                //ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string importFilePath = openFileDialog1.FileName;
                File.Copy(importFilePath,
               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + origin, true);

                p.IMAGE = origin;
                SaveProfileUpdateImage(p);


            }
        }
        
        public bool SaveProfileUpdateImage(Profile p)
        {
            if (p.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Active.ToString()))
            {
                if (p.CHECK_DATE_TO_LOCK && DateTime.Now.CompareTo(p.DATE_TO_LOCK) >= 0)
                {
                    //Today is later then date to lock
                    p.CHECK_DATE_TO_LOCK = false;
                    p.PROFILE_STATUS = GlobalConstant.ProfileStatus.Suspended.ToString();
                    p.DATE_MODIFIED = DateTime.Now;
                }
            }
            else
            {
                p.CHECK_DATE_TO_LOCK = false;
                p.DATE_MODIFIED = DateTime.Now;
            }

            if (SqliteDataAccess.UpdateDataProfile(p))
            {
                System.Windows.Forms.MessageBox.Show("Profile saved!");
                UpdateProfileToAllDevice(p);
                return true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Field with (*) is mandatory!");
                return false;
            }

        }

        private bool UpdateProfileToAllDevice(Profile p)
        {
            if (p != null)
            {
                int count = 0;
                List<int> listDeviceId = new List<int>();

                if (!String.IsNullOrEmpty(p.LIST_DEVICE_ID))
                {
                    string[] listVar = p.LIST_DEVICE_ID.Split(',');
                    foreach (string var in listVar)
                    {
                        int temp;
                        Int32.TryParse(var, out temp);
                        if (temp != 0)
                        {
                            listDeviceId.Add(temp);
                        }
                    }
                    foreach (int id in listDeviceId)
                    {
                        List<DeviceProfiles> getCloneDeviceProfile = SqliteDataAccess.LoadAllDeviceProfiles(id, "", "", p.PIN_NO);

                        foreach (DeviceProfiles DP in getCloneDeviceProfile)
                        {
                            DP.CloneDataFromProfile(p);

                            if (p.PROFILE_STATUS.Equals(GlobalConstant.ProfileStatus.Suspended.ToString()))
                            {
                                if (!DP.CLIENT_STATUS.Equals(GlobalConstant.ClientStatus.Deleted.ToString()))
                                {
                                    DP.CLIENT_STATUS = GlobalConstant.ClientStatus.Delete.ToString();
                                }
                            }
                            else
                            {
                                DP.CLIENT_STATUS = GlobalConstant.ClientStatus.Unknow.ToString();
                                if (DP.SERVER_STATUS.Equals(GlobalConstant.ServerStatus.None.ToString()))
                                {
                                    DP.SERVER_STATUS = GlobalConstant.ServerStatus.Update.ToString();
                                }
                            }

                            if (SqliteDataAccess.UpdateDataDeviceProfiles(id, DP))
                            {
                                count++;
                            }
                        }

                    }
                    if (count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private void ManageDeviceProfiles(Device p)
        {
            DeviceProfilesManagement deviceProfilesWindow = new DeviceProfilesManagement(p);
            deviceProfilesWindow.ShowDialog();
        }

        private void AddDevice()
        {
            AddDeviceWindow addDeviceWindow = new AddDeviceWindow();
            addDeviceWindow.ShowDialog();
        }

        private void EditDevice(Device p)
        {
            EditDeviceWindow editDeviceWindow = new EditDeviceWindow(p);
            editDeviceWindow.ShowDialog();
        }

        private bool RemoveDevice(Device p)
        {
            //Remove Device in database
            if (SqliteDataAccess.DeleteDataDevice(p))
            {
                //Succeed
                Console.WriteLine("Succeed");
                return true;
            }
            else
            {
                //Unsucceed
                Console.WriteLine("Unsucceed");
                return false;
            }
        }

        private void ReloadDataDeviceProfiles(Device p)
        {
            if (p != null)
            {
                try
                {
                    _deviceProfiles.Clear();
                    List<DeviceProfiles> deviceProfileList = SqliteDataAccess.LoadAllDeviceProfiles(p.DEVICE_ID);
                    foreach (DeviceProfiles item in deviceProfileList)
                    {
                        _deviceProfiles.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    logFile.Error(ex.Message);
                }
            }
        }

        private void DisconnectDevice(Device p)
        {
            if(p != null)
            {
                p.DeviceItem.Dispose();
            }
        }

        private void ConnectDevice(Device p)
        {
            if (p != null)
            {
                p.DeviceItem.checkAlive.Start();
                p.DeviceItem.Start("ws://" + p.DEVICE_IP + ":9090");
            }
        }

        private void SearchProfiles(ItemCollection p)
        {
            if (p != null)
            {
                p.Filter = (obj) => (
                (((Profile)obj).ADDRESS.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).AD_NO.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).EMAIL.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).PROFILE_NAME.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).PHONE.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).PIN_NO.ToLower().Contains(Search_profiles_others.ToString().ToLower()))
            );
            }
        }

        private void SearchDeviceProfiles(ItemCollection p)
        {
            if (p != null)
            {
                p.Filter = (obj) => (
                (((DeviceProfiles)obj).ADDRESS.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfiles)obj).AD_NO.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfiles)obj).EMAIL.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfiles)obj).PROFILE_NAME.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfiles)obj).PHONE.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfiles)obj).PIN_NO.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower()))
            );
            }
        }

        public void ManageClass()
        {
            ClassManagement classManagement = new ClassManagement();
            classManagement.ShowDialog();
        }

        public void ImportProfiles(List<CardType> classes)
        {
            if (classes != null && classes.Count > 0)
            {
                ImportWindow importWindow = new ImportWindow(classes);
                importWindow.ShowDialog();
            }
        }

        public void ReloadDataCardTypes()
        {
            try
            {
                _classes.Clear();
                List<CardType> classesList = SqliteDataAccess.LoadAllCardType();
                Classes.Add(new CardType(0, "All"));
                foreach (CardType item in classesList)
                {
                    _classes.Add(item);
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        public void ReloadDataDevices(Device removedDevice = null)
        {
            try
            {
                List<Device> deviceList = SqliteDataAccess.LoadAllDevices();
                foreach (Device item in deviceList)
                {
                    Device device = CheckExistDeviceRF(Devices, item);
                    if (device == null)
                    {
                        //Add Device
                        Devices.Add(item);
                    }
                    else
                    {
                        //Update Device
                        device.DEVICE_IP = item.DEVICE_IP;
                        device.DEVICE_NAME = item.DEVICE_NAME;
                        device.DEVICE_NOTE = item.DEVICE_NOTE;
                    }
                }
                //Remove Device
                if (removedDevice != null)
                {
                    Devices.Remove(removedDevice);
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        public Device CheckExistDeviceRF(ObservableCollection<Device> list, Device deviceRF)
        {
            foreach (Device item in list)
            {
                if ((item.DEVICE_ID == deviceRF.DEVICE_ID))
                {
                    //item.DEVICE_IP = deviceRF.DEVICE_IP;
                    //item.DEVICE_NAME = deviceRF.DEVICE_NAME;
                    //item.DEVICE_STATUS = deviceRF.DEVICE_STATUS;
                    return item;
                }
            }
            return null;
        }

        public void ReloadDataProfiles(string className = "", string subClass = "")
        {
            try
            {
                _profiles.Clear();
                List<Profile> profileList = SqliteDataAccess.LoadAllProfiles(className, subClass);
                foreach (Profile item in profileList)
                {
                    _profiles.Add(item);
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        public void ReloadDataDeviceProfiles(Device device, string className = "", string subClass = "")
        {
            try
            {
                _deviceProfiles.Clear();
                List<DeviceProfiles> deviceProfileList = SqliteDataAccess.LoadAllDeviceProfiles(device.DEVICE_ID, className, subClass);
                foreach (DeviceProfiles item in deviceProfileList)
                {
                    _deviceProfiles.Add(item);
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        public void ReloadDataTimeCheck(List<TimeRecord> timeCheckList)
        {
            try
            {
                _timeChecks.Clear();
                foreach (TimeRecord item in timeCheckList)
                {
                    _timeChecks.Add(item);
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        public void ReloadDataTimeCheck(Profile selectedProfile, DateTime selectedDate)
        {
            Console.WriteLine("Reload TimeCheck: "+selectedDate.ToShortDateString());
            if (selectedDate != null && selectedProfile != null)
            {
                List<TimeRecord> timeCheckList = SqliteDataAccess.LoadAllTimeCheck(selectedProfile.PIN_NO, selectedDate);
                _timeChecks.Clear();
                foreach (TimeRecord item in timeCheckList)
                {
                    _timeChecks.Add(item);
                }
            }
                
        }

        private bool CanEditOrRemoveDevice(Device p)
        {
            if (p != null && p.DeviceItem.WebSocketStatus != null)
            {
                if (p.DeviceItem.WebSocketStatus.Equals("Disconnected") && !p.DeviceItem.IsSendingProfiles)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool CheckIfDeviceCanConnect(Device p)
        {
            if (p != null)
            {
                if (p.DeviceItem.webSocket != null)
                {
                    if (p.DeviceItem.webSocket.IsAlive)
                    {
                        //p.DEVICE_STATUS = "Connected";
                    }
                    else
                    {
                        //p.DEVICE_STATUS = "Connecting";
                    }
                    return false;
                }
                else
                {
                    //p.DEVICE_STATUS = "Pending";
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public int CheckNoDeviceIsSyncing()
        {
            int _noDeviceSyncing = 0;
            foreach (Device item in Devices)
            {
                if(item.DeviceItem.IsSendingProfiles)
                {
                    _noDeviceSyncing++;
                }
            }
            return _noDeviceSyncing;
        }


        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ObservableCollection<Profile> _profiles = new ObservableCollection<Profile>();
        private ObservableCollection<Device> _devices = new ObservableCollection<Device>();
        private ObservableCollection<DeviceProfiles> _deviceProfiles = new ObservableCollection<DeviceProfiles>();
        private ObservableCollection<TimeRecord> _timeChecks = new ObservableCollection<TimeRecord>();
        private ObservableCollection<CardType> _classes = new ObservableCollection<CardType>();
        
        public ObservableCollection<Profile> Profiles => _profiles;
        public ObservableCollection<Device> Devices => _devices;
        public ObservableCollection<DeviceProfiles> DeviceProfiles => _deviceProfiles;
        public ObservableCollection<TimeRecord> TimeChecks => _timeChecks;
        public ObservableCollection<CardType> Classes => _classes;

        private Device _selectedDevice;
        private int _syncProgressValue;
        private int lastHour = DateTime.Now.Hour;
        private int lastSec = DateTime.Now.Second;
        private string _search_profiles_class;
        private string _search_profiles_group;
        private string _search_profiles_others;
        private string _search_deviceProfiles_class;
        private string _search_deviceProfiles_group;
        private string _search_deviceProfiles_others;
        private string _active_from_1;
        private string _active_to_1;
        private string _active_from_2;
        private string _active_to_2;
        private bool _activeTimeSchedule_1;
        private bool _activeTimeSchedule_2;
        private string _applyActiveTimeStatus;
        private int _exportProfilesProgressValue;
        public BackgroundWorker ExportWorker;
        private string _profilesWorkStatus;
        private bool _isExportingProfiles;
        private DateTime _selectedTimeCheckDate;


        public DateTime SelectedTimeCheckDate
        {
            get => _selectedTimeCheckDate;
            set
            {
                _selectedTimeCheckDate = value;
                RaisePropertyChanged("SelectedTimeCheckDate");
            }
        }

        public bool IsExportingProfiles
        {
            get => _isExportingProfiles;
            set
            {
                _isExportingProfiles = value;
                RaisePropertyChanged("IsExportingProfiles");
            }
        }

        public String ProfilesWorkStatus
        {
            get => _profilesWorkStatus;
            set
            {
                _profilesWorkStatus = value;
                RaisePropertyChanged("ProfilesWorkStatus");
            }
        }

        public int ExportProfilesProgressValue
        {
            get => _exportProfilesProgressValue;
            set
            {
                _exportProfilesProgressValue = value;
                RaisePropertyChanged("ExportProfilesProgressValue");
            }
        }
        
        public String ApplyActiveTimeStatus
        {
            get => _applyActiveTimeStatus;
            set
            {
                _applyActiveTimeStatus = value;
                RaisePropertyChanged("ApplyActiveTimeStatus");
            }
        }

        public int SyncProgressValue
        {
            get => _syncProgressValue;
            set
            {
                _syncProgressValue = value;
                RaisePropertyChanged("SyncProgressValue");
            }
        }

        public Device SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                RaisePropertyChanged("SelectedDevice");
            }
        }
        

        public Boolean ActiveTimeSchedule_1
        {
            get => _activeTimeSchedule_1;
            set
            {
                _activeTimeSchedule_1 = value;
                RaisePropertyChanged("ActiveTimeSchedule_1");
            }
        }

        public Boolean ActiveTimeSchedule_2
        {
            get => _activeTimeSchedule_2;
            set
            {
                _activeTimeSchedule_2 = value;
                RaisePropertyChanged("ActiveTimeSchedule_2");
            }
        }

        public String Search_profiles_class
        {
            get => _search_profiles_class;
            set
            {
                _search_profiles_class = value;
                RaisePropertyChanged("Search_profiles_class");
            }
        }

        public String Search_profiles_group
        {
            get => _search_profiles_group;
            set
            {
                _search_profiles_group = value;
                RaisePropertyChanged("Search_profiles_group");
            }
        }

        public String Search_profiles_others
        {
            get => _search_profiles_others;
            set
            {
                _search_profiles_others = value;
                RaisePropertyChanged("Search_profiles_others");
            }
        }

        public String Search_deviceProfiles_class
        {
            get => _search_deviceProfiles_class;
            set
            {
                _search_deviceProfiles_class = value;
                RaisePropertyChanged("Search_deviceProfiles_class");
            }
        }

        public String Search_deviceProfiles_group
        {
            get => _search_deviceProfiles_group;
            set
            {
                _search_deviceProfiles_group = value;
                RaisePropertyChanged("Search_deviceProfiles_group");
            }
        }

        public String Search_deviceProfiles_others
        {
            get => _search_deviceProfiles_others;
            set
            {
                _search_deviceProfiles_others = value;
                RaisePropertyChanged("Search_deviceProfiles_others");
            }
        }

        public String Active_from_1
        {
            get => _active_from_1;
            set
            {
                _active_from_1 = value;
                RaisePropertyChanged("Active_from_1");
            }
        }

        public String Active_to_1
        {
            get => _active_to_1;
            set
            {
                _active_to_1 = value;
                RaisePropertyChanged("Active_to_1");
            }
        }

        public String Active_from_2
        {
            get => _active_from_2;
            set
            {
                _active_from_2 = value;
                RaisePropertyChanged("Active_from_2");
            }
        }

        public String Active_to_2
        {
            get => _active_to_2;
            set
            {
                _active_to_2 = value;
                RaisePropertyChanged("Active_to_2");
            }
        }

        public List<DeviceProfiles> GetCanSyncDeviceProfiles(List<DeviceProfiles> p)
        {
            List<DeviceProfiles> CanSyncDeviceProfiles = p.FindAll((u) =>
            {
                switch (u.CLIENT_STATUS)
                {
                    case "Unknow":
                    {
                        if (
                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString())) ||

                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Update.ToString())) ||

                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString())) ||

                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString()))
                           )
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    case "Delete":
                    {
                        if (
                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.None.ToString())) ||

                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString())) ||

                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Update.ToString())) ||

                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Remove.ToString())) ||

                           ((u.PROFILE_STATUS == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                           (u.SERVER_STATUS == GlobalConstant.ServerStatus.Add.ToString()))
                           )
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    case "Deleted":
                    {
                        return false;
                    }
                    default:
                    {
                        return false;
                    }
                }
            });
            return CanSyncDeviceProfiles;
        }
    }

    public class ActiveTimeFrom1Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!String.IsNullOrEmpty(value.ToString()))
            {
                string[] parts = value.ToString().Split('-');
                return parts[0];
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    

    public class ActiveTimeTo1Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!String.IsNullOrEmpty(value.ToString()))
            {
                string[] parts = value.ToString().Split('-');
                return parts[1];
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    

    public class ActiveTimeFrom2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!String.IsNullOrEmpty(value.ToString()))
            {
                string[] parts = value.ToString().Split('-');
                return parts[2];
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    

    public class ActiveTimeTo2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!String.IsNullOrEmpty(value.ToString()))
            {
                string[] parts = value.ToString().Split('-');
                return parts[3];
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}