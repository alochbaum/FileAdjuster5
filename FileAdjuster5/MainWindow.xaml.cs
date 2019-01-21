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
    [Flags] public enum _eChecked : Int64
    {
        None        = 0,
        Headers     = 1 << 0,
        CombineFile = 1 << 1,
        NoDate      = 1 << 2,
        NoBracket   = 1 << 3,
        NoSecond    = 1 << 4,
        NoBlank     = 1 << 5
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private FileStream input;
        private FileStream output;
        private long lNullsNum = 0, lPosition = 0, 
            lFileSize = 0, lLinesPerFile = 0, lLastHistory =0, lLastAction=0;
        // This holds current out file
        private string strFileOut="";
        private BackgroundWorker MyWorker;
        // holds the count of null characters
        private Int64 iCountOfNulls = 0;
        // This is set for thread if working an include to allow next line to be check
        private bool blIncludeCheckNextLine = false;
        // If using File History this is set, so history isn't saved twice
        private bool blUsingHistory = true;
        // Same for Action History
        private bool blUsingActionsHistory = false;
        // Passes Combine Files to thread
        private Int64 I64_eChecked = 0;
        // Passes string extension to NextFile function when used in thread
        private string strExt = ".txt";
        // Private passes list of filenames to thread
        //private List<string> lFileList;
        private DateTime myStartTime;
        private DataTable MyDtable = new DataTable();

        public MainWindow()
        {
            InitializeComponent();
            // Adding the version number to the title
            MainFrame.Title = "File Adjuster version: " + Assembly.GetExecutingAssembly().GetName().Version;
            // Adding section to catch event when items are added to listbox of files
            ((INotifyCollectionChanged)lbFileNames.Items).CollectionChanged +=LbFileNames_CollectionChanged;
            log.Info($"{MainFrame.Title} is starting up.");
            // Setting up a worker thread
            MyWorker = (BackgroundWorker)this.FindResource("MyWorker");
            // displaying database and current working directory
            rtbStatus.AppendText($"Datafile directory:");
            rtbStatus.AppendText(FileAdjSQLite.DBFile() + "\r\n");
            rtbStatus.AppendText($"Program location {AppDomain.CurrentDomain.BaseDirectory}\r\n");
            // Get the Preset 0 in two parts first the actions then the checkboxes
            MyDtable = GetTable(0);
            dgActions.DataContext = MyDtable.DefaultView;
            // Setting checkboxes on for 2 items
            SetChecks(_eChecked.CombineFile|_eChecked.Headers);
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
            dlg.Multiselect = true;

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            if (result != null && result == true)
            {
                // Read the files
                foreach (String file in dlg.FileNames)
                {
                    AddFile(file);
                    log.Debug($"Added File {file}");
                }
            }
            else
            {
                if (result == null) log.Debug("File Open Dialog returned null");
            }
        }

        private void AddFile(string strFilename)
        {
            if (File.Exists(strFilename))
            {
                lbFileNames.Items.Add(strFilename);
            } else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("File doesn't exist",
                    "File not added because it doesn't exit",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // cblines stores the number of lines in format <num of lines>space some other text
            string strTemp = cbLines.SelectedValue.ToString();
            string[] words = strTemp.Split(' ');
            if(!Int64.TryParse(words[0],out lLinesPerFile))
            {
                log.Error("Error: Using default lines couldn't parse number of lines.");
                lLinesPerFile = 10000;
            }
            log.Debug($"Start button with line limit of {lLinesPerFile} ");
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
            if (!blUsingActionsHistory)SaveHistory();
            myStartTime = DateTime.Now;
            rtbStatus.AppendText($"Started work at {myStartTime.TimeOfDay}\r\n");
            int iCountListbox = lbFileNames.Items.Count;
            if (iCountListbox > 0)
            {
                // Load up passing private variable for thread
                strFileOut = tbOutFile.Text;
                strExt = tbExt.Text;
                I64_eChecked = CollectChecks();
                List<string> lFileList = new List<string>();
                // Clear number of null characters found in file
                iCountOfNulls = 0;
                for (int i = 0; i < iCountListbox; i++)
                {
                    lFileList.Add(lbFileNames.Items[i].ToString());
                      // passing extension because function below is also used in thread
                }
                MyWorker.RunWorkerAsync(lFileList);
            }
        }

        private Int64 CollectChecks()
        {
            _eChecked Ireturn = 0;
            if (cbxCombineFiles.IsChecked == true) Ireturn |= _eChecked.CombineFile;
            if (cbxFileHeaders.IsChecked == true) Ireturn |= _eChecked.Headers;
            if (cbxNoBracket.IsChecked == true) Ireturn |= _eChecked.NoBracket;
            if (cbxNoDate.IsChecked == true) Ireturn |= _eChecked.NoDate;
            if (cbxNoSecond.IsChecked == true) Ireturn |= _eChecked.NoSecond;
            if (cbxNoBlankLines.IsChecked == true) Ireturn |= _eChecked.NoBlank;
            return (Int64)Ireturn;
        }

        private void SetChecks(_eChecked InCheck)
        {
            if ((InCheck & _eChecked.CombineFile) != 0) cbxCombineFiles.IsChecked = true;
            else cbxCombineFiles.IsChecked = false;
            if ((InCheck & _eChecked.Headers) != 0) cbxFileHeaders.IsChecked = true;
            else cbxFileHeaders.IsChecked = false;
            if ((InCheck & _eChecked.NoBracket) != 0) cbxNoBracket.IsChecked = true;
            else cbxNoBracket.IsChecked = false;
            if ((InCheck & _eChecked.NoDate) != 0) cbxNoDate.IsChecked = true;
            else cbxNoDate.IsChecked = false;
            if ((InCheck & _eChecked.NoSecond) != 0) cbxNoSecond.IsChecked = true;
            else cbxNoSecond.IsChecked = false;
            if ((InCheck & _eChecked.NoBlank) != 0) cbxNoBlankLines.IsChecked = true;
            else cbxNoBlankLines.IsChecked = false;
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
                    log.Debug($"Dropped file {file}");
                }
            }
        }

        private void LbFileNames_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetOutFile();
        }

        private void SetOutFile()
        {
            if (lbFileNames.Items.Count > 0)
            {
                strExt = tbExt.Text;  // passing extension because function below is also used in thread
                // if comment checkbox is selected replated the incoming string
                if (cbxComment.IsChecked == true)
                {
                    DataRow drow = MyDtable.Rows[0];
                    string strTemp = drow.Field<string>("Parameter1");
                    strTemp = System.IO.Path.GetDirectoryName(lbFileNames.Items[0].ToString()) + "\\" +
                        strTemp.Substring(0, strTemp.IndexOf(' ')) + "-0" + strExt;
                    tbOutFile.Text = NextFreeFilename(strTemp);
                }
                else
                {
                    tbOutFile.Text = NextFreeFilename(lbFileNames.Items[0].ToString());
                }
            }
            else
            {
                tbOutFile.Text = "";
            }

        }

        private string NextFreeFilename(string inStr)
        {
            string strReturn = "";
            if (File.Exists(inStr))
            {
                do
                {
                    // the might have gone through one time and is repeated because it exists
                    if (strReturn.Length > 1) inStr = strReturn;
                    var baseDir = System.IO.Path.GetDirectoryName(inStr);
                    string strFile = System.IO.Path.GetFileNameWithoutExtension(inStr);
                    // this has file name might have at end starting with "-0";
                    int iTemp = strFile.LastIndexOf('-');
                    int iiTemp = 0;
                    // if you didn't find a hyphen do default
                    if (iTemp > 0)
                    {
                        string strFront = strFile.Substring(0, iTemp);
                        string strEnd = strFile.Substring(++iTemp, strFile.Length - iTemp);
                        bool successfullyParsed = int.TryParse(strEnd, out iiTemp);
                        if (successfullyParsed)
                        {
                            iiTemp++;
                            strReturn = baseDir + "\\" + strFront + "-" + iiTemp.ToString()
                                + strExt;
                        } else
                        {
                            strReturn = baseDir + "\\" + strFile + "-0" + tbExt.Text;
                        }
                    } else
                    {
                        strReturn = baseDir + "\\" + strFile +"-0" + tbExt.Text;
                    }
                } while (File.Exists(strReturn));
            } else { return inStr; }
            return strReturn;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFiles();
        }

        private void ClearFiles()
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
            string sTemp = "Got actions saved on: " +
                FileAdjSQLite.GetActionDate(lLastAction);
            log.Debug(sTemp);
            rtbStatus.AppendText(sTemp+"\r\n");
            blUsingActionsHistory = true;
        }

        private void SaveHistory()
        {
            Int64 lTemp = FileAdjSQLite.GetActionint();
            lTemp++;
            rtbStatus.AppendText($"Saving Action Grid As Group {lTemp}\r\n");
            foreach (DataRow myRow in MyDtable.Rows)
            {
                FileAdjSQLite.WriteAction(myRow.Field<Int64>("Order"),
                    lTemp, myRow.Field<string>("Action"),
                    myRow.Field<string>("Parameter1"),
                    myRow.Field<string>("Parameter2"));
                // updating on screen numbers
                myRow["Group"] = lTemp;
            }
            blUsingActionsHistory = false;
        }

        private void MyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> lInFiles = (List<string>)e.Argument;
            long lNumOfLines = 0;
            bool blWorkingInsideFileList = false;
            long lNumFiles = lInFiles.Count;
            long lCurNumFile = 0, lCurBytesRead = 0;
            byte[] inbuffer = new byte[16 * 4096];
            byte[] outbuffer = new byte[17 * 4096]; // Allowing for extended string buffer
            byte[] strbuffer = new byte[2003];
            int read, icount, iStrLen = 0;
            int iOut = 0;
            bool blHitLastLine = false;
            bool blLineOverRun = false;

            foreach (string sInFile in lInFiles)
            {
                if (File.Exists(sInFile))
                {
                    input = File.Open(sInFile, FileMode.Open);
                    lCurNumFile++;
                    // skipping opening new file if blWorkingInsideFileList and Combine
                    if (!(blWorkingInsideFileList && (!((I64_eChecked & (Int64)_eChecked.CombineFile) == 0))))
                    {
                        output = File.Open(strFileOut, FileMode.Create);
                    }

                    blWorkingInsideFileList = true;
                    lPosition = 0;  // stores output file position
                                    // writing header
                    if ((I64_eChecked & (Int64)_eChecked.Headers) != 0)
                    {
                        const string csBound = "========\r\n";
                        byte[] baBound = Encoding.ASCII.GetBytes(csBound);
                        output.Write(baBound, 0, 10);
                        byte[] baFile = Encoding.ASCII.GetBytes(sInFile + "\r\n");
                        output.Write(baFile, 0, baFile.Length);
                        output.Write(baBound, 0, 10);
                    }
                    FileInfo f = new FileInfo(sInFile);
                    lFileSize = f.Length;
                    iOut = read = icount = iStrLen = 0;
                    blHitLastLine = false;
                    blLineOverRun = false;

                    // looping the full file size
                    while ((read = input.Read(inbuffer, 0, inbuffer.Length)) > 0)
                    {
                        lCurBytesRead += 65376;
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
                                iCountOfNulls++;
                            } // End check for null bytes
                              // if you hit last line change the files
                            if (blHitLastLine)
                            {
                                blHitLastLine = false;
                                icount++;
                                outbuffer[iOut] = inbuffer[icount];
                                output.Write(outbuffer, 0, iOut);
                                output.Close();
                                strFileOut = NextFreeFilename(strFileOut);
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
                            MyWorker.ReportProgress(
                                 (int)(((double)lCurBytesRead / (double)lFileSize) * 100.0) +
                                 (int)(((double)lCurNumFile / (double)lNumFiles) * 100000.0));
                        } // end looping output files
                    } // finished reading last block
                      // finish writing out block
                    if (iOut > 0)
                    {
                        output.Write(outbuffer, 0, iOut);
                    }
                    if ((I64_eChecked & (Int64)_eChecked.CombineFile) == 0)
                        output.Close();
                    input.Close();
                    if (MyWorker.CancellationPending) e.Cancel = true;
                }
                else
                {
                    MyWorker.ReportProgress((int)(((double)lCurBytesRead / (double)lFileSize) * 100.0) +
                        (int)(((double)lCurNumFile / (double)lNumFiles) * 100000.0));
                }// end looping output files// end of input files
                if(output!=null)output.Close();  // closing combined files or if missed check above
            }
        }

        private bool DoICopyStr(string strIn)
        {
            if ((I64_eChecked & (Int64)_eChecked.NoBlank) != 0)
            {
                if (strIn == null || strIn.Length < 3) return false;
            }
            else
            {
                if (strIn == null || strIn.Length < 2) return false;
            }
            bool blReturn = true, blDoingInclude = false, blInsideTW = false;
            foreach (DataRow dRow in MyDtable.Rows)
            {
                // no use doing work if first parameter is empty
                if (dRow[3].ToString().Length > 1)
                {
                    // if doing include check for the flags
                    if (blIncludeCheckNextLine)
                    {

                        if((I64_eChecked & (Int64)_eChecked.NoBracket) != 0)
                        {
                            if (strIn[0] != '[') return true;
                        }
                        if ((I64_eChecked & (Int64)_eChecked.NoDate) != 0)
                        {
                            // Date can be 8/8/2018 to 11/11/2018 or 8 to 10 characters, so split off first 11 characters and look for space
                            string[] strSplit = strIn.Split(' ');
                            if (strSplit[0].Length > 7 && strSplit[0].Length < 11)
                            {
                                DateTime dateTime2;
                                if (!DateTime.TryParse(strSplit[0], out dateTime2)) return true;
                            } else { return true; }
                        }
                        if((I64_eChecked & (Int64)_eChecked.NoSecond) != 0)
                        {
                            if (dRow[4] != null && dRow[4].ToString().Length > 0)
                            {
                                if (strIn.IndexOf(dRow[4].ToString()) == 0) return true;
                            }
                        }
                    }
                    string sTemp = dRow[2].ToString();
                    switch (sTemp)
                    {
                        case "Exclude":
                            // check in includes ran out first
                            if (blDoingInclude && !blReturn)
                            {
                                blIncludeCheckNextLine = false;
                                return false;
                            }
                            if (strIn.IndexOf(dRow[3].ToString()) >= 0)
                            {
                                blIncludeCheckNextLine = false;
                                return false;
                            }
                            break;
                        case "Include":
                            // Found and include, reversing return logic for next couple
                            // of includes, flagging reversed logic for other values
                            if (!blDoingInclude)
                            {
                                blDoingInclude = true;
                                blReturn = false;
                            }
                            if (strIn.IndexOf(dRow[3].ToString()) >= 0)
                            {
                                blIncludeCheckNextLine = true;
                                return true;
                            }
                            break;
                        case "Any_Case_Exclude":
                            // check in includes ran out first
                            if (blDoingInclude && !blReturn) return false;
                            if (strIn.ToUpper().IndexOf(dRow[3].ToString().ToUpper()) >= 0)
                            {
                                blIncludeCheckNextLine = false;
                                return false;
                            }
                            break;
                        case "Any_Case_Include":
                            if (!blDoingInclude)
                            {
                                blDoingInclude = true;
                                blReturn = false;
                            }
                            if (strIn.ToUpper().IndexOf(dRow[3].ToString().ToUpper()) >= 0)
                            {
                                blIncludeCheckNextLine = true;
                                return true;
                            }
                            break;
                        // time case is type of include case
                        case "Time_Window":
                            if (!blDoingInclude)
                            {
                                blDoingInclude = true;
                                blReturn = false;
                            }
                            // need both dRow[3] and dRow[4] for the bounds
                            DateTime dtStartWin, dtEndWin;
                            try
                            {
                                dtStartWin = DateTime.Parse("1/1/0001 " + dRow[3].ToString().Trim());
                                dtEndWin = DateTime.Parse("1/1/0001 " + dRow[4].ToString().Trim());
                            }
                            catch (Exception errorCode)
                            {
                                log.Error("Error: Time Parsing " + errorCode.ToString() +" "+dRow[3].ToString()+" "+dRow[4].ToString());
                                blDoingInclude = false;
                                return false;
                            }
                            // some line don't start with number, if this is case check if blInsideTW, some new trace starts with [ characters
                            if (char.IsNumber(strIn[0]) == true || strIn[0] == '[')
                            {
                                blInsideTW = false;
                                int iFirstSpace = strIn.IndexOf(' ') + 1;
                                // Test if trace log of GPI or other service which ends in period not AM or PM
                                int iLastSpace = strIn.IndexOf('.');
                                if (iLastSpace < 15 || iLastSpace > 23) iLastSpace = strIn.IndexOf('M') + 1;
                                // the following might fail because of not containing time or can't parse time
                                try
                                {
                                    string stTime = strIn.Substring(iFirstSpace, iLastSpace - iFirstSpace);
                                   // log.Debug($"{strIn}=={stTime}");
                                    DateTime dtReadtime = DateTime.Parse("1/1/0001 " + stTime);
                                    // Time has to equal (0) or greater (positive) than start of window time to be in window
                                    if (DateTime.Compare(dtReadtime, dtStartWin) < 0) return false;
                                    // Time has to equal (0) or less (negative) than end of window time to be in window
                                    if (DateTime.Compare(dtReadtime, dtEndWin) > 0) return false;
                                    blReturn = blInsideTW = true;
                                }
                                catch (Exception errorCode)
                                {
                                    log.Error("Error " + errorCode.ToString() + " log.Error " + strIn);
                                    return false;
                                }
                            }
                            else if (!blInsideTW)
                            {// some line don't start with time, if this is case check if blInsideTW
                                blIncludeCheckNextLine = false;
                                return false;
                            }
                    
                            break;
                        default:
                            // Note: case comment does nothing
                            break;                
                    }
                }
            }
            // turning off the doing include if there is a false
            if (!blReturn) blIncludeCheckNextLine = false;
            return blReturn;
        }

        private void BtnOpenNotePad_Click(object sender, RoutedEventArgs e)
        {
            Process myProcess = new Process();
            try
            {
                Process.Start("notepad++.exe",$"\"{strFileOut}\"");
            }
            catch
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error",
                    "Tried to open " + strFileOut + " in Notepad++.exe but failed",
                    MessageBoxButton.OK,MessageBoxImage.Error);
                log.Error($"Failed Notepad++ open of {strFileOut}");
            }
        }

        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            // need to have clear set group, so there has to be first comment line
            AddRow myAddRow = new AddRow();
            if(myAddRow.ShowDialog()==true)
            {
                blUsingActionsHistory = false;
                List<string> inString = myAddRow.GetSettings();
                int i = MyDtable.Rows.Count;
                DataRow drow = MyDtable.Rows[--i];
                Int64 iOrder = drow.Field<Int64>("Order");
                // need to get last order number to increase by one
                MyDtable.Rows.Add(++iOrder, drow.Field<Int64>("Group"), inString[0], inString[1], inString[2]);
            }
            myAddRow.Close();
        }

        private void BtnClearRows_Click(object sender, RoutedEventArgs e)
        {
            GetString myGet = new GetString("Enter Comment String","A new set of actions start with a comment");
            if(myGet.ShowDialog()==true)
            {
                blUsingActionsHistory = false;
                MyDtable.Rows.Clear();
                Int64 i = FileAdjSQLite.GetActionint();
                MyDtable.Rows.Add(1, ++i, "Comment", myGet.GetAnswer(), "");
                log.Info("Cleared action");
            }
        }

        private void BtnSavePreset_Click(object sender, RoutedEventArgs e)
        {
            if (!blUsingActionsHistory) SaveHistory();
            I64_eChecked = CollectChecks();
            Int64 iGroup = MyDtable.Rows[0].Field<Int64>(1);
            SavePreset mySavePreset = new SavePreset(iGroup,I64_eChecked);
            if (mySavePreset.ShowDialog() == true)
            {
                string sTemp = $"Saved preset for group {iGroup}";
                rtbStatus.AppendText($"{sTemp}\r\n");
                log.Info(sTemp);
            }
        }

        private void BtnLoadPreset_Click(object sender, RoutedEventArgs e)
        {
            FindPreset MyFindPreset = new FindPreset();
            if (MyFindPreset.ShowDialog() == true)
            {
                Int64 iTemp = MyFindPreset.GetGroup();
                MyDtable = GetTable(iTemp);
                dgActions.DataContext = MyDtable.DefaultView;
                Int64 iTemp2 = FileAdjSQLite.GetPresetFlags(iTemp);
                SetChecks((_eChecked)iTemp2);
            }
        }

        private void BtnSwapAbove_Click(object sender, RoutedEventArgs e)
        {
            if(dgActions.SelectedIndex<1)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Operational Hint-Left click on row",
                    "You have to select a row below top row",
                    MessageBoxButton.OK,MessageBoxImage.Exclamation);
            }
            else
            {
                DataTable dtParent = MyDtable;
                int iOldRowIndex = dgActions.SelectedIndex;
                DataRow row = MyDtable.Rows[iOldRowIndex];
                DataRow newRow = dtParent.NewRow();
                newRow.ItemArray = row.ItemArray;
                int iOrderNew = int.Parse( newRow["Order"].ToString())-1;
                newRow["Order"] = iOrderNew;
                if (iOldRowIndex >0 && iOldRowIndex <= dtParent.Rows.Count)
                {
                    dtParent.Rows.Remove(row);
                    dtParent.Rows.InsertAt(newRow, iOldRowIndex - 1);
                    MyDtable.Rows[iOldRowIndex]["Order"] = iOrderNew +1;
                }
                dgActions.DataContext = MyDtable.DefaultView;
            }
        }

        private void BtnQuickInsert_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                string clipboardText = Clipboard.GetText(TextDataFormat.Text);
                QuickAddRow("Include", clipboardText);
            } else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Can't read clipboard",
                    "Can't insert Include action from clipboard text",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void QuickAddRow(string strAction, string strParam1)
        {
            blUsingActionsHistory = false;
            int i = MyDtable.Rows.Count;
            DataRow drow = MyDtable.Rows[--i];
            Int64 iOrder = drow.Field<Int64>("Order");
            // need to get last order number to increase by one
            MyDtable.Rows.Add(++iOrder, drow.Field<Int64>("Group"), strAction, strParam1, "");

        }

        private void BtnQuickExclude_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                string clipboardText = Clipboard.GetText(TextDataFormat.Text);
                QuickAddRow("Exclude", clipboardText);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Can't read clipboard",
                    "Can't insert Include action from clipboard text",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

        }

        private void BtnOpenDir_Click(object sender, RoutedEventArgs e)
        {
            if(tbOutFile.Text.Length >1)
            {
                string strDir = System.IO.Path.GetDirectoryName(tbOutFile.Text);
                if(strDir.Length > 1)
                {
                    System.Diagnostics.Process.Start(strDir);
                } else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Can't open directory",
                        "Program didn't parse directory from output file.",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            } else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Can't open directory",
                    "This button is linked to output file, and it is empty.",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void BtnOut2In_Click(object sender, RoutedEventArgs e)
        {
            ClearFiles();
            AddFile(strFileOut);
        }

        private void btnExportPresets_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Title = "Enter database file to save presets to",
                Filter = "sqlite|*.sqlite"
            };
            if (dlg.ShowDialog() == true)
            {
                if (File.Exists(dlg.FileName)) File.Delete(dlg.FileName);
                FileAdjSQLite.SavePresets(dlg.FileName);               
            }
        }

        private void CbxComment_Checked(object sender, RoutedEventArgs e)
        {
            SetOutFile();
        }

        private void CbxComment_Unchecked(object sender, RoutedEventArgs e)
        {
            SetOutFile();
        }

        private void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            string strfilename = $"{ AppDomain.CurrentDomain.BaseDirectory }\\FileAdjuster5.log";
            Process myProcess = new Process();
            try
            {

                Process.Start("notepad++.exe", strfilename);
            }
            catch
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error",
                    "Tried to open " + strfilename + " in Notepad++.exe but failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelRow_Click(object sender, RoutedEventArgs e)
        {
            if (MyDtable.Rows.Count < 2)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Operational Training",
                    "Sorry you can't delete last row.",MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
            else
            {
                int iTemp = dgActions.SelectedIndex;
                if (iTemp > 0 && iTemp < MyDtable.Rows.Count)
                {
                    MyDtable.Rows[iTemp].Delete();
                }
                else Xceed.Wpf.Toolkit.MessageBox.Show("Operational Hint-Left click on row", 
                    "You have to select a valid row.",MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }
        
        void MyWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int i = e.ProgressPercentage;
            //log.Debug($"Progress of {i}");
            int ii = i % 100;
            pbProgress.Value = ii;
            if (i > 1000)
                pbFiles.Value = (i - ii) / 1000;
            else pbFiles.Value = 0;
        }
        void MyWorker_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            pbProgress.Value = 0;
            pbFiles.Value = 0;
            tbOutFile.Text = strFileOut;
            btnStart.IsEnabled = true;
            btnOpenNotePad.IsEnabled = true;
            btnCancel.IsEnabled = false;
            if (iCountOfNulls > 0) rtbStatus.AppendText($"Found {iCountOfNulls} null characters in files which weren't copied");
            TimeSpan mySpan = DateTime.Now - myStartTime;
            rtbStatus.AppendText($"{DateTime.Now.TimeOfDay} Finished with file in {mySpan.Seconds} seconds\r\n");
        }
    
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            MyWorker.CancelAsync();
        }
    }
}
