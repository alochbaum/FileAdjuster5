using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace FileAdjuster5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log =
    log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private FileStream input;
        private FileStream output;
        private long lNullsNum = 0, lPosition = 0, 
            lFileSize = 0, lLinesPerFile = 0, lLastHistory =0, lLastAction=0;
        // This holds current out file
        private string strFileOut="";
        private BackgroundWorker MyWorker;
        private string strRTB = "";
        private bool blPosNum = true;
        // If using File History this is set, so history isn't saved twice
        private bool blUsingHistory = true;
        // This stores extension for creating output files
        private bool blUsingActions = false;
        private string strExt = ".txt";
        private DateTime myStartTime;
        private DataTable MyDtable = new DataTable();

        public MainWindow()
        {
            InitializeComponent();
            // Adding the version number to the title
            MainFrame.Title = "File Adjuster version:" + Assembly.GetExecutingAssembly().GetName().Version;
            // Adding section to catch event when items are added to listbox of files
            ((INotifyCollectionChanged)lbFileNames.Items).CollectionChanged +=
    LbFileNames_CollectionChanged;
            log.Info("FileAdjuster is starting up.");
            // Setting up a worker thread
            MyWorker = (BackgroundWorker)this.FindResource("MyWorker");
            tbOutFile.Text = AppDomain.CurrentDomain.BaseDirectory;
            rtbStatus.AppendText($"Datafile directory:");
            rtbStatus.AppendText(FileAdjSQLite.DBFile() + "\r\n");
            rtbStatus.AppendText($"Program location {AppDomain.CurrentDomain.BaseDirectory}\r\n");
            // Get the DataTable.
            MyDtable = GetTable(0);
            dgActions.DataContext = MyDtable.DefaultView;

        }
        static DataTable GetTable(Int64 iGroup)
        {
            DataTable table = FileAdjSQLite.ReadActions(iGroup);
            return table;
        }
        private void CbLines_Loaded(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = FileAdjSQLite.GetSizes();

            // ... Assign the ItemsSource to the List.
            cbLines.ItemsSource = data;

            // ... Make the first item selected.
            cbLines.SelectedIndex = 0;
        }

        private void BtnAddFile_Click(object sender, RoutedEventArgs e)
        {
            blUsingHistory = false;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select File to Add To List (It will not auto start)",
                Filter = "All Files|*.*"
            };

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                //tbDBLocation.Text = dlg.FileName;
                lbFileNames.Items.Add(dlg.FileName);
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            strFileOut = tbOutFile.Text;
            strExt = tbExt.Text;
            // cblines stores the number of lines to work 
            string strTemp = cbLines.SelectedValue.ToString();
            string[] words = strTemp.Split(' ');
            if(!Int64.TryParse(words[0],out lLinesPerFile))
            {
                // Just for testing
                lLinesPerFile = 10000;
            }
            btnCancel.IsEnabled = true;
            btnOpenNotePad.IsEnabled = false;
            btnStart.IsEnabled = false;
            if (!blUsingHistory)
            {
                Int64 iTemp = FileAdjSQLite.GetHistoryint();
                iTemp++;
                rtbStatus.AppendText($"Saving File History #{iTemp}\r\n");
                for (int i = 0; i < lbFileNames.Items.Count; i++)
                {
                    FileAdjSQLite.WriteHistory(iTemp, lbFileNames.Items[i].ToString(),tbExt.Text);
                }
            }
            Int64 lTemp = FileAdjSQLite.GetActionint();
            lTemp++;
            if (blUsingActions)
            {
                rtbStatus.AppendText("Saving Action Grid \r\n");
                foreach (DataRow myRow in MyDtable.Rows)
                {
                    FileAdjSQLite.WriteAction(myRow.Field<Int64>("Order"),
                        lTemp, myRow.Field<string>("Action"),
                        myRow.Field<string>("Parameter1"),
                        myRow.Field<string>("Parameter2"));
                }
            }
            myStartTime = DateTime.Now;
            rtbStatus.AppendText($"Started work at {myStartTime.TimeOfDay}\r\n");
            MyWorker.RunWorkerAsync(lbFileNames.Items[0].ToString());
            //MessageBox.Show("Started");
        }

        private void StackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string file in files)
                {
                    lbFileNames.Items.Add(file);
                }
            }
        }

        private void LbFileNames_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (lbFileNames.Items.Count > 0)
            {
                string strTemp = lbFileNames.Items[0].ToString();
                // for root directories like e:\ they will come in with \
                var baseDir = System.IO.Path.GetDirectoryName(strTemp);
                string strFile = System.IO.Path.GetFileNameWithoutExtension(strTemp) + "-0";
                tbOutFile.Text = baseDir + "\\" + strFile + tbExt.Text;
            } else
            {
                tbOutFile.Text = "";
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            blUsingHistory = false;
            tbExt.Text = ".txt";
            lLastHistory = 0;
            lbFileNames.Items.Clear();
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            blUsingHistory = true;
            if (lLastHistory < 2) lLastHistory = FileAdjSQLite.GetHistoryint();
            else lLastHistory--;
            List<string> lsTemp = FileAdjSQLite.ReadHistory(lLastHistory);
            lbFileNames.Items.Clear();
            rtbStatus.Document.Blocks.Clear();
            
            foreach(string s in lsTemp)
            {
                string[] sTemp = s.Split('|');
                tbExt.Text = sTemp[1];
                rtbStatus.AppendText("Read History: "+sTemp[0]+" created on "+sTemp[2]+"\r\n\r\n");
                lbFileNames.Items.Add(sTemp[0]);
            }
        }

        private void BtnActionHistory_Click(object sender, RoutedEventArgs e)
        {
            if (lLastAction < 2) lLastAction = FileAdjSQLite.GetActionint();
            else lLastAction--;
            MyDtable = GetTable(lLastAction);
            dgActions.DataContext = MyDtable.DefaultView;
            rtbStatus.AppendText("Got actions saved on: " + FileAdjSQLite.GetActionDate(lLastAction)
                +"\r\n");
        }


        private void MyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            input = File.Open(e.Argument.ToString(), FileMode.Open);
            output = File.Open(strFileOut, FileMode.Create);
            long lNumOfLines = 0;
            lPosition = 0;
            FileInfo f = new FileInfo(e.Argument.ToString());
            lFileSize = f.Length;
            byte[] inbuffer = new byte[16 * 4096];
            byte[] outbuffer = new byte[17 * 4096]; // Allowing for extended string buffer
            byte[] strbuffer = new byte[2003];
            string strHoldLast50 = "";
            int read, icount, iStrLen=0;
            long lStoredPosition = 0;
            bool blHitLastLine = false;
            bool blLineOverRun = false;
      
            int iOut = 0;
            // looping the full file size
            while ((read = input.Read(inbuffer, 0, inbuffer.Length)) > 0)
            {
                    iOut = 0;
                    // looping the input buffer
                    for (icount = 0; icount < read; icount++)
                    {
                        // Start checking for null bytes
                        if (inbuffer[icount] != 0)
                        {
                            if (iStrLen > 1999)
                            {
                                strbuffer[iStrLen++] = (byte)'\r';
                                strbuffer[iStrLen++] = (byte)'\n';
                                blLineOverRun = true;
                            }
                            else
                                strbuffer[iStrLen++] = inbuffer[icount];
                            if (inbuffer[icount] == '\n' || blLineOverRun)
                            {
                                blLineOverRun = false;
                                if (DoICopyStr(System.Text.Encoding.Default.GetString(strbuffer, 0, iStrLen)))
                                {
                                    // Insert string testing section here
                                    Array.Copy(strbuffer, 0, outbuffer, iOut, iStrLen);
                                    iOut += iStrLen;
                                    lNumOfLines++;
                                    if (lNumOfLines >= lLinesPerFile)
                                    {
                                        blHitLastLine = true;
                                    }
                                }
                                // reset string weither I copy it or not
                                iStrLen = 0;
                            }
                        }
                        else // Found a null byte in file
                        {
                            if (lPosition == lStoredPosition)
                                lNullsNum++;
                            else
                            {
                               {
                                    strRTB = strHoldLast50;
                                    if (blPosNum)
                                    {
                                        string s1 = $"Position {lStoredPosition} has {lNullsNum} null characters";
                                        strRTB += s1 + "\r\n";
                                    }
                                    lNullsNum = -1;
                                }
                                lNullsNum++;
                            }
                            lStoredPosition = lPosition;
                        } // End check for null bytes
                        // if you hit last line change the files
                        if (blHitLastLine)
                        {
                            blHitLastLine = false;
                            icount++;
                            outbuffer[iOut] = inbuffer[icount];
                            output.Write(outbuffer, 0, iOut);
                            output.Close();
                            strFileOut = ComputeNewFileOut(strFileOut);
                            output = File.Open(strFileOut, FileMode.Create);
                            lNumOfLines = 0;
                            iOut = 0;
                        }
                    }  // end looping input buffer
                // writing to output buffer
                if (iOut > 0)
                {
                    output.Write(outbuffer, 0, iOut);
                    lPosition += read;
                    // checking to see if user click cancel, if they did get out of loop
                    if (MyWorker.CancellationPending) break;
                    MyWorker.ReportProgress((int)(((double)lPosition / (double)lFileSize) * 100.0));
                } // end looping output files
            } // finished reading last block
            // finish writing out block
            if (iOut > 0)
            {
                output.Write(outbuffer, 0, iOut);
            }
            output.Close();
            input.Close();
            if (MyWorker.CancellationPending) e.Cancel = true;
        }

        private bool DoICopyStr(string strIn)
        {
            bool blReturn = true, blDoingInclude = false;
            foreach(DataRow dRow in MyDtable.Rows)
            {
                if (dRow[2].Equals("Exclude") && (dRow[3].ToString().Length > 1))
                {
                    if (strIn.IndexOf(dRow[3].ToString()) >= 0) return false;
                }
                else if (dRow[2].Equals("Include"))
                {
                    // This is set first time in to the Include, so if there are additional includes
                    // They will not be excluded
                    if (!blDoingInclude)
                    {
                        blDoingInclude = true;
                        blReturn = false;
                    }
                    if (strIn.IndexOf(dRow[3].ToString()) >= 0)
                    {
                        return true;
                    }
                }
            }
            return blReturn;
        }

        private void btnOpenNotePad_Click(object sender, RoutedEventArgs e)
        {
            Process myProcess = new Process();
            try
            {
                Process.Start("notepad++.exe", strFileOut);
            }
            catch
            {
                MessageBox.Show("Tried to open " + strFileOut + " in Notepad++.exe but failed");
            }
        }

        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            // need to have clear set group, so there has to be first comment line
            AddRow myAddRow = new AddRow();
            if(myAddRow.ShowDialog()==true)
            {
                blUsingActions = true;
                List<string> inString = myAddRow.GetSettings();
                int i = MyDtable.Rows.Count;
                DataRow drow = MyDtable.Rows[--i];
                Int64 iOrder = drow.Field<Int64>("Order");
                // need to get last order number to increase by one
                MyDtable.Rows.Add(++iOrder, drow.Field<Int64>("Group"), inString[0], inString[1], inString[2]);
            }
            myAddRow.Close();
        }

        private void btnClearRows_Click(object sender, RoutedEventArgs e)
        {
            GetString myGet = new GetString("Enter Comment String","A new set of actions start with a comment");
            if(myGet.ShowDialog()==true)
            {
                blUsingActions = false;
                MyDtable.Rows.Clear();
                Int64 i = FileAdjSQLite.GetActionint();
                MyDtable.Rows.Add(1, ++i, "Comment", myGet.GetAnswer(), "");
            }
        }

        private void BtnSavePreset_Click(object sender, RoutedEventArgs e)
        {
            Int64 iGroup = MyDtable.Rows[0].Field<Int64>(1);
            SavePreset mySavePreset = new SavePreset();
            mySavePreset.iGroupID = iGroup;
            if (mySavePreset.ShowDialog() == true)
                rtbStatus.AppendText($"Saved preset for group {iGroup}\r\n");
        }

        private void btnDelRow_Click(object sender, RoutedEventArgs e)
        {
            if (MyDtable.Rows.Count < 2)
            {
                MessageBox.Show("Sorry you can't delete last row.");
            }
            else
            {
                int iTemp = dgActions.SelectedIndex;
                if (iTemp > 0 && iTemp < MyDtable.Rows.Count)
                {
                    MyDtable.Rows[iTemp].Delete();
                }
                MessageBox.Show("You have to select a valid row.");
            }
        }

        private string ComputeNewFileOut(string inStr)
        {
            string strOut = "";
            var baseDir = System.IO.Path.GetDirectoryName(inStr);
            string strFile = System.IO.Path.GetFileNameWithoutExtension(inStr);
            // this has file name plus number at end starting with "-0";
            int iTemp = strFile.LastIndexOf('-');
            string strFront = strFile.Substring(0, iTemp);
            string strEnd = strFile.Substring(++iTemp, strFile.Length - iTemp);
            int iiTemp = int.Parse(strEnd);
            iiTemp++;
            strOut = baseDir + "\\" + strFront +"-"+ iiTemp.ToString()
                + strExt;
            return strOut;
        }
        void MyWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbProgress.Value = e.ProgressPercentage;
        }
        void MyWorker_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            pbProgress.Value = 0;
            tbOutFile.Text = strFileOut;
            btnStart.IsEnabled = true;
            btnOpenNotePad.IsEnabled = true;
            btnCancel.IsEnabled = false;
            TimeSpan mySpan = DateTime.Now - myStartTime;
            rtbStatus.AppendText($"{DateTime.Now.TimeOfDay} Finished with file in {mySpan.Seconds} seconds\r\n");
        }
    
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            MyWorker.CancelAsync();
        }
    }
}
