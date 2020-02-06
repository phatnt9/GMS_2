using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace GateAccessControl.Views
{
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string importFilePath = "";
        private string importFileFolder = "";
        private bool isAddProfile = true; //add-true, update-false
        private List<CardType> classes;
        private BackgroundWorker ImortWorker;

        public ImportWindow(List<CardType> classes)
        {
            InitializeComponent();
            btn_stop.IsEnabled = false;
            this.classes = classes;
            Closed += ImportWindow_Closed;
        }

        private void ImportWindow_Closed(object sender, EventArgs e)
        {
            if (ImortWorker != null && ImortWorker.IsBusy)
            {
                ImortWorker.CancelAsync();
            }
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Title = "Browse Excel Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "Excel",
                Filter = "All Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx",
                FilterIndex = 2,
                RestoreDirectory = true
                //ReadOnlyChecked = true,
                //ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtFile.Text = importFilePath = openFileDialog1.FileName;
                importFileFolder = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
                string fileName = "DataImport.xlsx";
                string path = System.IO.Path.Combine(Environment.CurrentDirectory, fileName);
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pbStatus.Value = 0;
                if (string.IsNullOrEmpty(this.txtFile.Text))
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(GlobalConstant.messageValidate, "File", "File"), GlobalConstant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!File.Exists(this.txtFile.Text))
                {
                    System.Windows.Forms.MessageBox.Show("File not Exist!", GlobalConstant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                btn_import.IsEnabled = false;
                isAddProfile = (bool)rb_add.IsChecked ? true : false;
                ImortWorker = new BackgroundWorker();
                ImortWorker.WorkerSupportsCancellation = true;
                ImortWorker.WorkerReportsProgress = true;
                ImortWorker.DoWork += Worker_DoWork;
                ImortWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                ImortWorker.ProgressChanged += Worker_ProgressChanged;
                ImortWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                btn_import.IsEnabled = true;
                logFile.Error(ex.Message);
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // check error, check cancel, then use result
            if (e.Error != null)
            {
                // handle the error
                processStatusText.Content = "Error";
            }
            else if (e.Cancelled)
            {
                // handle cancellation
                processStatusText.Content = "";
            }
            else
            {
            }
            // general cleanup code, runs when there was an error or not.
            pbStatus.Value = 0;
            //mainW.mainModel.ReloadListProfileRFDGV();
        }

        private DateTime ParseDateTimeFormCell(string sDate)
        {
            DateTime ngayThang = DateTime.MinValue;
            double dateD;
            try
            {
                ngayThang = DateTime.MinValue;
                ngayThang = DateTime.ParseExact(sDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                dateD = double.Parse(sDate);
                var dateTime = DateTime.FromOADate(dateD).ToString("MMMM dd, yyyy");
                ngayThang = DateTime.Parse(dateTime);
            }
            return ngayThang;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(importFilePath);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            try
            {
                GlobalConstant.CreateFolderToSaveData();
                this.Dispatcher.Invoke(() =>
                {
                    processStatusText.Content = "Loading";
                    btn_stop.IsEnabled = true;
                });
                int rowCount = xlRange.Rows.Count;
                int colCount = xlRange.Columns.Count;
                string serialId = "";
                for (int i = 2; i <= rowCount; i++)
                {
                    if (xlRange.Cells[i, 2] != null && xlRange.Cells[i, 2].Value2 != null)
                    {
                        Profile profile = new Profile();
                        DateTime ngayThang = DateTime.MinValue;

                        if (xlRange.Cells[i, 8] != null && xlRange.Cells[i, 8].Value2 != null)
                        {
                            serialId = xlRange.Cells[i, 8].Value2.ToString();
                            profile.pinno = serialId;
                        }

                        profile.adno = xlRange.Cells[i, 3].Value2.ToString().ToUpper();

                        profile.profileName = xlRange.Cells[i, 2].Value2.ToString().ToUpper();

                        profile.className = xlRange.Cells[i, 9].Value2.ToString();

                        profile.subClass = (xlRange.Cells[i, 10].Value2 == null) ? "" : xlRange.Cells[i, 10].Value2.ToString();

                        profile.gender = (xlRange.Cells[i, 4].Value2.ToString() == "Male" ? "Male" : "Female");

                        profile.dob = ParseDateTimeFormCell(xlRange.Cells[i, 5].Value2.ToString());

                        profile.disu = ParseDateTimeFormCell(xlRange.Cells[i, 6].Value2.ToString());

                        profile.email = (xlRange.Cells[i, 11].Value2 == null) ? "" : xlRange.Cells[i, 11].Value2.ToString();

                        profile.address = (xlRange.Cells[i, 12].Value2 == null) ? "" : xlRange.Cells[i, 12].Value2.ToString();

                        profile.phone = (xlRange.Cells[i, 13].Value2 == null) ? "" : xlRange.Cells[i, 13].Value2.ToString();

                        profile.profileStatus = xlRange.Cells[i, 14].Value2.ToString();

                        profile.image = xlRange.Cells[i, 7].Value2.ToString();

                        if (xlRange.Cells[i, 16].Value2 != null)
                        {
                            profile.check_date_to_lock = Boolean.Parse(xlRange.Cells[i, 16].Value2.ToString());
                        }
                        else
                        {
                            profile.check_date_to_lock = false;
                        }

                        if (profile.check_date_to_lock == true)
                        {
                            if (xlRange.Cells[i, 15].Value2 != null)
                            {
                                profile.date_to_lock = ParseDateTimeFormCell(xlRange.Cells[i, 15].Value2.ToString());
                            }
                            else
                            {
                                profile.date_to_lock = DateTime.MinValue;
                            }
                        }
                        else
                        {
                            profile.date_to_lock = DateTime.MinValue;
                        }

                        profile.license_plate = (xlRange.Cells[i, 17].Value2 == null) ? "" : xlRange.Cells[i, 17].Value2.ToString();

                        if (isAddProfile)
                        {
                            //Add
                            profile.date_created = DateTime.Now;
                            profile.date_modified = DateTime.Now;
                        }
                        else
                        {
                            //Update
                            profile.date_created = (xlRange.Cells[i, 18].Value2 == null) ? DateTime.Now : ParseDateTimeFormCell(xlRange.Cells[i, 18].Value2.ToString());
                            profile.date_modified = DateTime.Now;
                        }

                        try
                        {
                            if (!CheckClassNameValid(classes, profile.className))
                            {
                                //Create New Class
                                CardType NewClass = new CardType()
                                {
                                    className = profile.className
                                };
                                SqliteDataAccess.InsertCardTypesAsync(NewClass);
                                //Add or Update Profile
                                if (isAddProfile)
                                {
                                    SqliteDataAccess.InsertProfileAsync(profile);
                                    //if (SqliteDataAccess.InsertProfile(profile))
                                    //{
                                    //    ImportProfileImage(importFileFolder, profile.image);
                                    //}
                                }
                                else
                                {
                                    SqliteDataAccess.UpdateProfileAsync(profile);
                                    //if (SqliteDataAccess.UpdateProfile(profile))
                                    //{
                                    //    ImportProfileImage(importFileFolder, profile.image);
                                    //}
                                }
                            }
                            else
                            {
                                //Add or Update Profile
                                if (isAddProfile)
                                {
                                    SqliteDataAccess.InsertProfileAsync(profile);
                                    //if (SqliteDataAccess.InsertProfile(profile))
                                    //{
                                    //    ImportProfileImage(importFileFolder, profile.image);
                                    //}
                                }
                                else
                                {
                                    SqliteDataAccess.UpdateProfileAsync(profile);
                                    //if (SqliteDataAccess.UpdateProfile(profile))
                                    //{
                                    //    ImportProfileImage(importFileFolder, profile.image);
                                    //}
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logFile.Error(ex.Message);
                        }
                    }
                    if (ImortWorker.CancellationPending)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            processStatusText.Content = "Stopped";
                            btn_stop.IsEnabled = false;
                        });
                        break;
                    }
                    (sender as BackgroundWorker).ReportProgress((i * 100) / rowCount);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Import error, please check again!", GlobalConstant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                logFile.Error(ex.Message);
            }
            finally
            {
                //Constant.mainWindowPointer.WriteLog("Dong xlWorkbook.Close();");
                xlWorkbook.Close();
                xlApp.Quit();
                this.Dispatcher.Invoke(() =>
                {
                    processStatusText.Content = "Finished";
                    btn_import.IsEnabled = true;
                    btn_stop.IsEnabled = false;
                });
            }
        }

        public bool CheckClassNameValid(List<CardType> list, string ClassName)
        {
            foreach (CardType item in list)
            {
                if (item.className.Equals(ClassName))
                {
                    return true;
                }
            }
            Console.WriteLine("Class name is not valid in database");
            return false;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            ImortWorker.CancelAsync();
        }

        public void ImportProfileImage(string importFolderPath, string imageName)
        {
            try
            {
                GlobalConstant.CreateFolderToSaveData();
                string path = importFolderPath + @"\Image";
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
                File.Copy(importFolderPath + @"\Image\" + imageName,
                    GlobalConstant.ImagePath + imageName,
                    true);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }
    }
}