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
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static GateAccessControl.DeviceItem;

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
        public ICommand RefreshDeviceCommand { get; set; }
        public ICommand SelectDeviceCommand { get; set; }
        public ICommand ConnectDeviceCommand { get; set; }
        public ICommand DisconnectDeviceCommand { get; set; }
        public ICommand RequesTimeRecordCommand { get; set; }

        public ICommand AddProfileCommand { get; set; }
        public ICommand EditProfileCommand { get; set; }
        public ICommand RemoveProfileCommand { get; set; }
        public ICommand RefreshProfileCommand { get; set; }
        public ICommand SelectProfileCommand { get; set; }
        public ICommand ExportProfilesCommand { get; set; }
        public ICommand StopExportProfilesCommand { get; set; }

        public ICommand DeviceProfilesManageCommand { get; set; }
        public ICommand SelectDeviceProfilesCommand { get; set; }
        public ICommand RefreshDeviceProfilesCommand { get; set; }

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
            ReloadDevices();
            ReloadProfiles();
            ReloadCardTypesAsync();
            SyncProgressValue = 0;
            CreateCheckSuspendProfilesTimer();
            CreateRequestTimeChecksTimer();

            RefreshDeviceCommand = new RelayCommand<Device>(
                 (p) =>
                 {
                     return CheckNoDeviceIsSyncing();
                 },
                 (p) =>
                 {
                     ReloadDevices();
                 });

            RefreshProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return (CheckNoDeviceIsSyncing() && IsExportingProfiles == false);
                 },
                 (p) =>
                 {
                     ReloadProfiles();
                 });

            RefreshDeviceProfilesCommand = new RelayCommand<Device>(
                 (p) =>
                 {
                     if (SelectedDevice != null)
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
                     ReloadDeviceProfilesAsync(SelectedDevice);
                 });

            SelectNextDateCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return (SelectedProfile != null && SelectedTimeCheckDate != null);
                 },
                 (p) =>
                 {
                     SelectedTimeCheckDate = SelectedTimeCheckDate.AddDays(1);
                     ReloadDataTimeCheckAsync(SelectedProfile, SelectedTimeCheckDate);
                 });

            SelectPreviousDateCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return (SelectedProfile != null && SelectedTimeCheckDate != null);
                 },
                 (p) =>
                 {
                     SelectedTimeCheckDate = SelectedTimeCheckDate.AddDays(-1);
                     ReloadDataTimeCheckAsync(SelectedProfile, SelectedTimeCheckDate);
                 });

            SelectedDateChangeCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return (SelectedProfile != null && SelectedTimeCheckDate != null);
                 },
                 (p) =>
                 {
                     //ReloadDataTimeCheck(SelectedProfile, SelectedTimeCheckDate);
                 });

            RequesTimeRecordCommand = new RelayCommand<Device>(
                 (p) =>
                 {
                     return (SelectedDevice != null &&
                     SelectedDevice.DeviceItem.WebSocketStatus == RosStatus.Connected.ToString() &&
                     SelectedDevice.DeviceItem.IsSendingProfiles == false);
                 },
                 (p) =>
                 {
                     SelectedDevice.DeviceItem.RequestPersonListImmediately();
                 });

            StopExportProfilesCommand = new RelayCommand<Device>(
                 (p) =>
                 {
                     //Can Stop when sending Profiles
                     return IsExportingProfiles;
                 },
                 (p) =>
                 {
                     ExportWorker.CancelAsync();
                 });

            ExportProfilesCommand = new RelayCommand<List<Profile>>(
                 (p) =>
                 {
                     return (p != null && p.Count > 0 && IsExportingProfiles == false);
                 },
                 (p) =>
                 {
                     ExportProfiles(p);
                 });

            SelectDeviceProfilesCommand = new RelayCommand<DeviceProfile>(
                 (p) =>
                 {
                     return true;
                 },
                 (p) =>
                 {
                     ParseActiveTimeDeviceProfile(SelectedDeviceProfile);
                 });

            SetTimeDeviceProfileCommnad = new RelayCommand<List<DeviceProfile>>(
                 (p) =>
                 {
                     if (p != null && p.Count > 0 && SelectedDevice != null)
                     {
                         return CheckNoDeviceIsSyncing();
                     }
                     else
                     {
                         return false;
                     }
                 },
                 (p) =>
                 {
                     SetTimeDeviceProfileAsync(p);
                 });

            AddProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return true;
                 },
                 (p) =>
                 {
                     AddProfile(p);
                     ReloadProfiles();
                 });

            EditProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     //Cannot when any device is syncing
                     return (SelectedProfile != null && CheckNoDeviceIsSyncing());
                 },
                 (p) =>
                 {
                     EditProfile(SelectedProfile);
                     ReloadProfiles();
                     if (SelectedDevice != null)
                     {
                         ReloadDeviceProfilesAsync(SelectedDevice);
                     }
                 });

            RemoveProfileCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     //Cannot when any device is syncing
                     return (SelectedProfile != null && CheckNoDeviceIsSyncing() && String.IsNullOrEmpty(SelectedProfile.list_device_id));
                 },
                 (p) =>
                 {
                     if (System.Windows.Forms.MessageBox.Show
                       (
                       String.Format(GlobalConstant.messageDeleteConfirm, "Profile"),
                       GlobalConstant.messageTitileWarning, MessageBoxButtons.YesNo,
                       MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes
                       )
                     {
                         RemoveProfileAsync(SelectedProfile);
                     }
                 });

            SelectProfileCommand = new RelayCommand<List<TimeRecord>>(
                 (p) =>
                 {
                     return true;
                 },
                 (p) =>
                 {
                     ReloadDataTimeCheckAsync(SelectedProfile, SelectedTimeCheckDate);
                     ReloadProfileImage(SelectedProfile);
                 });

            ReplaceProfileImageCommand = new RelayCommand<Profile>(
                 (p) =>
                 {
                     return (SelectedProfile != null);
                 },
                 (p) =>
                 {
                     ReplaceProfileImage(SelectedProfile.image, SelectedProfile);
                     ReloadProfileImage(SelectedProfile);
                     ReloadDeviceProfilesAsync(SelectedDevice);
                 });

            StopSyncCommand = new RelayCommand<Device>(
                 (p) =>
                 {
                     //Can Stop when sending Profiles
                     return (SelectedDevice != null && SelectedDevice.DeviceItem.IsSendingProfiles);
                 },
                 (p) =>
                 {
                     SelectedDevice.SyncWorker.CancelAsync();
                 });

            SyncCommand = new RelayCommand<List<DeviceProfile>>(
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
                         if (SelectedDevice.DeviceItem.IsSendingProfiles || SelectedDevice.DeviceItem.WebSocketStatus != RosStatus.Connected.ToString())
                         {
                             return false;
                         } //right

                         //if (SelectedDevice.DeviceItem.IsSendingProfiles)
                         //{
                         //    return false;
                         //} //wrong

                         return (GetCanSyncDeviceProfiles(p).Count > 0);
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
                     ReloadDevices();
                 });

            EditDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return CanEditOrRemoveDevice(SelectedDevice);
                },
                (p) =>
                {
                    EditDevice(SelectedDevice);
                    //ReloadDevices();
                });

            RemoveDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return CanEditOrRemoveDevice(SelectedDevice);
                },
                (p) =>
                {
                    if (System.Windows.Forms.MessageBox.Show
                        (
                        String.Format(GlobalConstant.messageDeleteConfirm, "Device"),
                        GlobalConstant.messageTitileWarning, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes
                        )
                    {
                        RemoveDeviceAsync(SelectedDevice);
                    }
                });

            SelectDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return true;
                },
                (p) =>
                {
                    if (SelectedDevice != null)
                    {
                        SelectDeviceAsync();
                    }
                });

            DeviceProfilesManageCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return (SelectedDevice != null && SelectedDevice.DeviceItem.IsSendingProfiles == false);
                },
                (p) =>
                {
                    ManageDeviceProfiles(SelectedDevice);
                    ReloadProfiles();
                    ReloadDeviceProfilesAsync(SelectedDevice);
                });

            ConnectDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    return CheckIfDeviceCanConnect(SelectedDevice);
                },
                (p) =>
                {
                    ConnectDevice(SelectedDevice);
                });

            DisconnectDeviceCommand = new RelayCommand<Device>(
                (p) =>
                {
                    if (SelectedDevice == null || SelectedDevice.DeviceItem.IsSendingProfiles)
                    {
                        return false;
                    }
                    else
                    {
                        return !CheckIfDeviceCanConnect(SelectedDevice);
                    }
                },
                (p) =>
                {
                    DisconnectDevice(SelectedDevice);
                });

            SearchClassProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadProfiles();
                });

            SearchGroupProfilesCommand = new RelayCommand<ItemCollection>(
                 (p) => true,
                 (p) =>
                 {
                     ReloadProfiles();
                 });

            SearchOthersProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    SearchProfiles(p);
                });

            SearchClassDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDeviceProfilesAsync(SelectedDevice);
                });

            SearchGroupDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    ReloadDeviceProfilesAsync(SelectedDevice);
                });

            SearchOthersDeviceProfilesCommand = new RelayCommand<ItemCollection>(
                (p) => true,
                (p) =>
                {
                    SearchDeviceProfiles(p);
                });

            ImportProfilesCommand = new RelayCommand<Profile>(
                (p) =>
                {
                    if (IsExportingProfiles == false)
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
                    ReloadCardTypesAsync();
                    IEnumerable<CardType> obsCollection = Classes;
                    List<CardType> list = new List<CardType>(obsCollection);
                    ImportProfiles(list);
                    ReloadProfiles();
                    ReloadCardTypesAsync();
                });

            ManageClassCommand = new RelayCommand<CardType>(
                (p) => true,
                (p) =>
                {
                    ManageClass();
                    ReloadCardTypesAsync();
                });
        }

        private void ReloadProfileImage(Profile p)
        {
            BackgroundWorker LoadImageWorker = new BackgroundWorker();
            LoadImageWorker.WorkerSupportsCancellation = true;
            LoadImageWorker.WorkerReportsProgress = true;
            LoadImageWorker.DoWork += LoadImageWorker_DoWork;
            LoadImageWorker.RunWorkerCompleted += LoadImageWorker_RunWorkerCompleted;
            LoadImageWorker.ProgressChanged += LoadImageWorker_ProgressChanged;
            LoadImageWorker.RunWorkerAsync(p);
        }

        private void LoadImageWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void LoadImageWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void LoadImageWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            long elapsedMs;
            Profile p = e.Argument as Profile;
            // the code that you want to measure comes here
            if (p != null)
            {
                string imagePath = GlobalConstant.ImagePath + "\\" + p.image.ToString();
                string defaultImagePath = GlobalConstant.ImagePath + "\\default.png";
                File.Exists(imagePath);
                if (File.Exists(imagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        ProfileImage = bitmap;
                    });
                }
                else
                {
                    if (File.Exists(defaultImagePath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.UriSource = new Uri(defaultImagePath);
                        bitmap.EndInit();
                        bitmap.Freeze();
                        Dispatcher.CurrentDispatcher.Invoke(() =>
                        {
                            ProfileImage = bitmap;
                        });
                    }
                    else
                    {
                    }
                }
            }
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Load Picture time:" + elapsedMs.ToString());
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

        private async void ExportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsExportingProfiles = true;
            ProfilesWorkStatus = AppStatus.Exporting.ToString();
            List<object> genericList = e.Argument as List<object>;
            Excel.Application excel = new Excel.Application();
            Excel.Workbook workbook = excel.Workbooks.Add(Type.Missing);
            Excel.Worksheet worksheet = null;
            try
            {
                Task<List<CardType>> loadTask = SqliteDataAccess.LoadCardTypesAsync();
                List<CardType> listClasses = await loadTask;

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
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = i + 1; }
                        if (j == 1)//Name
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].profileName; }
                        if (j == 2)//Adno
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].adno;
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
                            cell.Value2 = profileList[i].gender.ToString();
                            cell.Locked = true;
                            cell.Validation.IgnoreBlank = true;
                            cell.Validation.InCellDropdown = true;
                        }
                        if (j == 4)//DOB
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].dob; }
                        if (j == 5)//Disu
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].disu; }
                        if (j == 6)//Image
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].image;
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].IMAGE;
                        }
                        if (j == 7)//PIN No.
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].pinno;
                            //worksheet.Range[cellColumnIndex].NumberFormat = "0";
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].PIN_NO;
                        }

                        if (j == 8)//Class
                        {
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].CLASS;

                            var list = new List<string>();
                            foreach (CardType classes in listClasses)
                            {
                                list.Add(classes.className);
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
                            cell.Value2 = profileList[i].className;
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
                            cell.Value2 = profileList[i].subClass;
                            cell.Validation.IgnoreBlank = true;
                            cell.Validation.InCellDropdown = true;
                        }

                        if (j == 10)//Email
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].email; }
                        if (j == 11)//Address
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].address; }
                        if (j == 12)//Phone
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].phone;
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
                            cell.Value2 = profileList[i].profileStatus;
                            cell.Locked = true;
                            cell.Validation.IgnoreBlank = true;
                            cell.Validation.InCellDropdown = true;
                        }

                        if (j == 14)//Expire Date
                        {
                            if (profileList[i].check_date_to_lock == true)
                            {
                                { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].date_to_lock; }
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
                            cell.Value2 = profileList[i].check_date_to_lock;
                            cell.Locked = true;
                            cell.Validation.IgnoreBlank = true;
                            cell.Validation.InCellDropdown = true;
                        }
                        if (j == 16)//License plate
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].license_plate; }
                        if (j == 17)//Date created
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].date_created; }
                        if (j == 18)//Date modified
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].date_modified; }

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

        private async void SetTimeDeviceProfileAsync(List<DeviceProfile> p)
        {
            string myActiveTime = GetActiveTimeFromTextBoxes();
            if (!String.IsNullOrEmpty(myActiveTime))
            {
                foreach (DeviceProfile item in p)
                {
                    item.active_time = myActiveTime;
                    if (item.profileStatus == GlobalConstant.ProfileStatus.Active.ToString() &&
                        item.server_status == GlobalConstant.ServerStatus.None.ToString())
                    {
                        item.server_status = GlobalConstant.ServerStatus.Update.ToString();
                    }
                    Task<bool> updateTask = SqliteDataAccess.UpdateDeviceProfileAsync(SelectedDevice.deviceId, item);
                    if (await updateTask)
                    {
                        ApplyActiveTimeStatus = "Success!";
                    }
                    else
                    {
                        ApplyActiveTimeStatus = "Unsuccess!";
                    }
                }
                ReloadProfiles();
            }
        }

        private string GetActiveTimeFromTextBoxes()
        {
            string returnStr = "";
            int count = 0;
            if (ActiveTimeSchedule_1)
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

        private void ParseActiveTimeDeviceProfile(DeviceProfile p)
        {
            if (p != null && !String.IsNullOrEmpty(p.active_time))
            {
                string[] listVar = p.active_time.Split('-');
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
            if (CheckNoDeviceIsSyncing())
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
            //SuspendStudentCheckTimer.Start();//wrong
        }

        private async void SuspendStudentCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (CheckNoDeviceIsSyncing())
            {
                if (lastHour < DateTime.Now.Hour || (lastHour == 23 && DateTime.Now.Hour == 0))
                {
                    lastHour = DateTime.Now.Hour;
                    Console.WriteLine("Catch " + DateTime.Now.ToLongTimeString() + "s.");

                    Task<bool> checkResultTask = CheckSuspendAllProfile();
                    bool check = await checkResultTask;
                    if (check)
                    {
                        Console.WriteLine("Success Check");
                    }
                    else
                    {
                        Console.WriteLine("Fail Check");
                    }
                }
            }
            else
            {
                Console.WriteLine("An Device is Syncing");
            }
        }

        public async Task<bool> CheckSuspendAllProfile()
        {
            try
            {
                //Get all Profile
                List<Profile> profiles = await SqliteDataAccess.LoadProfilesAsync("", "", "");

                //Check status --> check date to Suspend --> Suspend(active)
                foreach (Profile profile in profiles)
                {
                    if (profile.profileStatus.Equals(GlobalConstant.ProfileStatus.Active.ToString()) &&
                        profile.check_date_to_lock == true)
                    {
                        if (DateTime.Now > profile.date_to_lock)
                        {
                            profile.check_date_to_lock = false;
                            profile.profileStatus = GlobalConstant.ProfileStatus.Suspended.ToString();
                            profile.date_modified = DateTime.Now;
                            if (await SqliteDataAccess.UpdateProfileAsync(profile))
                            {
                                UpdateProfileToAllDeviceAsync(profile);
                            }
                            else
                            {
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
            finally
            {
                ReloadProfiles();
            }
        }

        private async void RemoveProfileAsync(Profile p)
        {
            if (String.IsNullOrEmpty(p.list_device_id))
            {
                Task<bool> deleteTask = SqliteDataAccess.DeleteProfileAsync(p);
                if (await deleteTask)
                {
                    Profiles.Remove(p);
                }
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
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string importFilePath = openFileDialog1.FileName;
                File.Copy(importFilePath,
               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + origin, true);

                p.image = origin;
                SaveProfileUpdateImage(p);
            }
        }

        public async void SaveProfileUpdateImage(Profile p)
        {
            if (p.profileStatus.Equals(GlobalConstant.ProfileStatus.Active.ToString()))
            {
                if (p.check_date_to_lock && DateTime.Now.CompareTo(p.date_to_lock) >= 0)
                {
                    //Today is later then date to lock
                    p.check_date_to_lock = false;
                    p.profileStatus = GlobalConstant.ProfileStatus.Suspended.ToString();
                    p.date_modified = DateTime.Now;
                }
            }
            else
            {
                p.check_date_to_lock = false;
                p.date_modified = DateTime.Now;
            }

            if (await SqliteDataAccess.UpdateProfileAsync(p))
            {
                System.Windows.Forms.MessageBox.Show("Profile saved!");
                UpdateProfileToAllDeviceAsync(p);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Field with (*) is mandatory!");
            }
        }

        private async Task<bool> UpdateProfileToAllDeviceAsync(Profile p)
        {
            if (p != null)
            {
                int count = 0;
                List<int> listDeviceId = new List<int>();

                if (!String.IsNullOrEmpty(p.list_device_id))
                {
                    string[] listVar = p.list_device_id.Split(',');
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
                        List<DeviceProfile> getCloneDeviceProfile = await SqliteDataAccess.LoadDeviceProfilesAsync(id, "", "", p.pinno);

                        foreach (DeviceProfile DP in getCloneDeviceProfile)
                        {
                            DP.CloneDataFromProfile(p);
                            if (p.profileStatus.Equals(GlobalConstant.ProfileStatus.Suspended.ToString()))
                            {
                                if (!DP.client_status.Equals(GlobalConstant.ClientStatus.Deleted.ToString()))
                                {
                                    DP.client_status = GlobalConstant.ClientStatus.Delete.ToString();
                                }
                            }
                            else
                            {
                                DP.client_status = GlobalConstant.ClientStatus.Unknow.ToString();
                                if (DP.server_status.Equals(GlobalConstant.ServerStatus.None.ToString()))
                                {
                                    DP.server_status = GlobalConstant.ServerStatus.Update.ToString();
                                }
                            }

                            Task<bool> updateTask = SqliteDataAccess.UpdateDeviceProfileAsync(id, DP);
                            if (await updateTask)
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

        private void ManageDeviceProfiles(Device d)
        {
            DeviceProfilesManagement deviceProfilesWindow = new DeviceProfilesManagement(d);
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

        private async void RemoveDeviceAsync(Device p)
        {
            if (CanEditOrRemoveDevice(p))
            {
                //Remove Device in database
                if (await SqliteDataAccess.DeleteDeviceAsync(p))
                {
                    //Succeed
                    Console.WriteLine("Succeed");
                    Devices.Remove(SelectedDevice);
                }
                else
                {
                    //Unsucceed
                    Console.WriteLine("Unsucceed");
                }
            }
        }

        private void DisconnectDevice(Device p)
        {
            if (p != null)
            {
                p.DeviceItem.Dispose();
            }
        }

        private void ConnectDevice(Device p)
        {
            if (p != null)
            {
                p.DeviceItem.WebSocketStatus = RosStatus.Pending.ToString();
                p.DeviceItem.checkAlive.Start();
                p.DeviceItem.Start("ws://" + p.deviceIp + ":9090");
            }
        }

        private void SearchProfiles(ItemCollection p)
        {
            if (p != null)
            {
                p.Filter = (obj) => (
                (((Profile)obj).address.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).adno.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).email.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).profileName.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).phone.ToLower().Contains(Search_profiles_others.ToString().ToLower())) ||
                (((Profile)obj).pinno.ToLower().Contains(Search_profiles_others.ToString().ToLower()))
            );
            }
        }

        private void SearchDeviceProfiles(ItemCollection p)
        {
            if (p != null)
            {
                p.Filter = (obj) => (
                (((DeviceProfile)obj).address.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfile)obj).adno.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfile)obj).email.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfile)obj).profileName.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfile)obj).phone.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower())) ||
                (((DeviceProfile)obj).pinno.ToLower().Contains(Search_deviceProfiles_others.ToString().ToLower()))
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

        public async void ReloadCardTypesAsync()
        {
            Task<List<CardType>> loadTask = SqliteDataAccess.LoadCardTypesAsync();
            List<CardType> classesList = await loadTask;
            classesList.Insert(0, new CardType(-1, "All"));
            Classes = new ObservableCollection<CardType>(classesList);
        }

        /// <summary>
        /// Return true if remove Device Success
        /// </summary>
        /// <param name="removedDevice"></param>
        /// <returns></returns>
        public async Task<bool> ReloadDevices(Device removedDevice = null)
        {
            try
            {
                List<Device> deviceList = await SqliteDataAccess.LoadDevicesAsync(0);
                foreach (Device item in deviceList)
                {
                    Device device = CheckExistDevice(Devices, item);
                    if (device == null)
                    {
                        //Add Device
                        Devices.Add(item);
                    }
                    else
                    {
                        //Update Device
                        device.deviceIp = item.deviceIp;
                        device.deviceName = item.deviceName;
                        device.deviceNote = item.deviceNote;
                    }
                }
                //Remove Device
                if (removedDevice != null)
                {
                    return Devices.Remove(removedDevice);
                }
                return false;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        public Device CheckExistDevice(ObservableCollection<Device> list, Device devices)
        {
            foreach (Device item in list)
            {
                if ((item.deviceId == devices.deviceId))
                {
                    return item;
                }
            }
            return null;
        }

        public async Task<bool> ReloadDataTimeCheckAsync(Profile selectedProfile, DateTime selectedDate)
        {
            Console.WriteLine("Reload TimeCheck: " + selectedDate.ToShortDateString());
            if (selectedDate != null && selectedProfile != null)
            {
                Task<List<TimeRecord>> loadTask = SqliteDataAccess.LoadTimeChecksAsync(selectedProfile.pinno, selectedDate);
                List<TimeRecord> list = await loadTask;
                TimeChecks = new ObservableCollection<TimeRecord>(list);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CanEditOrRemoveDevice(Device p)
        {
            if (p != null && p.DeviceItem.WebSocketStatus != null)
            {
                if (p.DeviceItem.WebSocketStatus == RosStatus.Disconnected.ToString() && p.DeviceItem.IsSendingProfiles == false)
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

        public bool CheckIfDeviceCanConnect(Device d)
        {
            if (d != null)
            {
                if (d.DeviceItem.webSocket != null)
                {
                    if (d.DeviceItem.webSocket.IsAlive)
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

        public bool CheckNoDeviceIsSyncing()
        {
            int _noDeviceSyncing = 0;
            foreach (Device item in Devices)
            {
                //Console.WriteLine("Device: "+item.DEVICE_NAME +" - IsSyncing: "+item.DeviceItem.IsSendingProfiles);
                if (item.DeviceItem.IsSendingProfiles)
                {
                    _noDeviceSyncing++;
                }
            }
            return (_noDeviceSyncing == 0) ? true : false;
        }

        public async void ReloadProfiles()
        {
            if (Search_profiles_class == null)
            {
                Search_profiles_class = "All";
            }
            if (Search_profiles_group == null)
            {
                Search_profiles_group = "All";
            }
            string type = Search_profiles_class == "All" ? "" : Search_profiles_class;
            string group = Search_profiles_group == "All" ? "" : Search_profiles_group;
            try
            {
                Profiles = new ObservableCollection<Profile>(await SqliteDataAccess.LoadProfilesAsync(type, group, ""));
                return;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return;
            }
        }

        public async void SelectDeviceAsync()
        {
            Task<bool> createTask = SqliteDataAccess.CreateDeviceProfilesTableAsync(SelectedDevice.deviceId);
            if (await createTask)
            {
                ReloadDeviceProfilesAsync(SelectedDevice);
            }
        }

        public async void ReloadDeviceProfilesAsync(Device d)
        {
            if (d == null)
            {
                return;
            }
            if (Search_deviceProfiles_class == null)
            {
                Search_deviceProfiles_class = "All";
            }
            if (Search_deviceProfiles_group == null)
            {
                Search_deviceProfiles_group = "All";
            }
            string type = Search_deviceProfiles_class == "All" ? "" : Search_deviceProfiles_class;
            string group = Search_deviceProfiles_group == "All" ? "" : Search_deviceProfiles_group;

            Task<List<DeviceProfile>> loadTask = SqliteDataAccess.LoadDeviceProfilesAsync(d.deviceId, type, group, "");
            List<DeviceProfile> list = await loadTask;
            DeviceProfiles = new ObservableCollection<DeviceProfile>(list);
        }

        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ObservableCollection<Profile> _profiles = new ObservableCollection<Profile>();
        private ObservableCollection<Device> _devices = new ObservableCollection<Device>();
        private ObservableCollection<DeviceProfile> _deviceProfiles = new ObservableCollection<DeviceProfile>();
        private ObservableCollection<TimeRecord> _timeChecks = new ObservableCollection<TimeRecord>();
        private ObservableCollection<CardType> _classes = new ObservableCollection<CardType>();

        public ObservableCollection<Profile> Profiles
        {
            get => _profiles;
            set
            {
                _profiles = value;
                RaisePropertyChanged("Profiles");
            }
        }

        public ObservableCollection<Device> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                RaisePropertyChanged("Devices");
            }
        }

        public ObservableCollection<DeviceProfile> DeviceProfiles
        {
            get => _deviceProfiles;
            set
            {
                _deviceProfiles = value;
                RaisePropertyChanged("DeviceProfiles");
            }
        }

        public ObservableCollection<TimeRecord> TimeChecks
        {
            get => _timeChecks;
            set
            {
                _timeChecks = value;
                RaisePropertyChanged("TimeChecks");
            }
        }

        public ObservableCollection<CardType> Classes
        {
            get => _classes;
            set
            {
                _classes = value;
                RaisePropertyChanged("Classes");
            }
        }

        private Device _selectedDevice;
        private Profile _selectedProfile;
        private DeviceProfile _selectedDeviceProfile;
        private DateTime _selectedTimeCheckDate;
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
        private BitmapImage _profileImage;

        public BitmapImage ProfileImage
        {
            get => _profileImage;
            set
            {
                _profileImage = value;
                RaisePropertyChanged("ProfileImage");
            }
        }

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

        public Profile SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                _selectedProfile = value;
                RaisePropertyChanged("SelectedProfile");
            }
        }

        public DeviceProfile SelectedDeviceProfile
        {
            get => _selectedDeviceProfile;
            set
            {
                _selectedDeviceProfile = value;
                RaisePropertyChanged("SelectedDeviceProfile");
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

        public List<DeviceProfile> GetCanSyncDeviceProfiles(List<DeviceProfile> p)
        {
            List<DeviceProfile> CanSyncDeviceProfiles = p.FindAll((u) =>
            {
                switch (u.client_status)
                {
                    case "Unknow":
                    {
                        if (
                           ((u.profileStatus == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.server_status == GlobalConstant.ServerStatus.Add.ToString())) ||

                           ((u.profileStatus == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.server_status == GlobalConstant.ServerStatus.Update.ToString())) ||

                           ((u.profileStatus == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.server_status == GlobalConstant.ServerStatus.Remove.ToString())) ||

                           ((u.profileStatus == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                           (u.server_status == GlobalConstant.ServerStatus.Add.ToString()))
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
                           ((u.profileStatus == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.server_status == GlobalConstant.ServerStatus.None.ToString())) ||

                           ((u.profileStatus == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.server_status == GlobalConstant.ServerStatus.Add.ToString())) ||

                           ((u.profileStatus == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.server_status == GlobalConstant.ServerStatus.Update.ToString())) ||

                           ((u.profileStatus == GlobalConstant.ProfileStatus.Active.ToString()) &&
                           (u.server_status == GlobalConstant.ServerStatus.Remove.ToString())) ||

                           ((u.profileStatus == GlobalConstant.ProfileStatus.Suspended.ToString()) &&
                           (u.server_status == GlobalConstant.ServerStatus.Add.ToString()))
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
            if (value == null)
            {
                return "";
            }
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
            if (value == null)
            {
                return "";
            }
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
            if (value == null)
            {
                return "";
            }
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
            if (value == null)
            {
                return "";
            }
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