using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        NoBlank     = 1 << 5,
        UseNum      = 1 << 6
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
        // private bool blUsingFileHist = true;
        // Same for Action History
        private bool blUsingActionsHistory = false;
        // If set file is in the On Air Mode, we need to follow up thread
        private bool blUsingOnAirMode = false;
        // Passes Combine Files to thread
        private Int64 I64_eChecked = 0;
        // Passes string extension to NextFile function when used in thread
        private string strExt = ".txt";
        // Private passes list of filenames to thread
        private DateTime myStartTime;
        private DataTable MyDtable = new DataTable();
        private List<jobReport> myRport = new List<jobReport>();
        // Holds last action for repeated use of add rows button
        private string strLastActionType = "Any_Case_Include";
        // String separating file names when processing many files
        const string csBound = "========\r\n";
        // This stores limit of numbers and current value of Number
        private int iNumLimit = 0, iNumAdditionLinesAfterInclude = 0;
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

            // Setting checkboxes on for 2 items, why is this not set in data with preset?
            SetChecks(_eChecked.CombineFile|_eChecked.Headers);
            // Check size of Log file
            
        }

        /// <summary>
        /// Read in Actions from the SQLite3 DB in to Action Table
        /// </summary>
        /// <param name="iGroup">Group number of Action Rows to retrive</param>
        /// <returns></returns>
        static DataTable GetTable(Int64 iGroup)
        {
            DataTable table = FileAdjSQLite.ReadActions(iGroup);
            return table;
        }

        #region Source File Section

        /// <summary>
        /// This is File section dropping action for user to drag from windows
        /// 1 or more files to fill actions, these dropped files are logged
        /// </summary>
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
            else
                Xceed.Wpf.Toolkit.MessageBox.Show(
                    "If you think these are files and not text, they might be from zip archive windows is showing you. Check to see if you need to unzip.",
                    "Can't read clipboard files", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// When files are dropped on clear button instead of whole area
        /// the old list of files is first cleared then new files are added
        /// </summary>
        private void BtbClear_Drop(object sender, DragEventArgs e)
        {
            //String[] myList = (String[])e.Data.GetData("FileDrop");

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Clearing Files First
                ClearFiles();
                // The other drop event will add the files
            }

        }

        private void BtnAddFile_Click(object sender, RoutedEventArgs e)
        {
            //blUsingFileHist = false;

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

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFiles();
        }

        private void ClearFiles()
        {
            //blUsingFileHist = false;
            tbExt.Text = ".txt";
            lLastHistory = 0;
            lbFileNames.Items.Clear();
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            //blUsingFileHist = true;
            if (lLastHistory < 2) lLastHistory = FileAdjSQLite.GetFileHistoryInt();
            else lLastHistory--;
            List<string> lsTemp = FileAdjSQLite.ReadHistory(lLastHistory);
            lbFileNames.Items.Clear();
            rtbStatus.Document.Blocks.Clear();
            string sReport = "Error Reading File History: no files.";  // outputs error if not replaced
            foreach (string s in lsTemp)
            {
                string[] sTemp = s.Split('|');
                tbExt.Text = sTemp[1];
                sReport = "Read File History: " + sTemp[0] + " created on " + sTemp[2];
                lbFileNames.Items.Add(sTemp[0]);
            }
            log.Debug(sReport);
            rtbStatus.AppendText(sReport + "\r\n");
        }

        private void BtnFileHist_Click(object sender, RoutedEventArgs e)
        {
            HIstoryWin myHwin = new HIstoryWin(false);
            myHwin.ShowDialog();
            Int64 iGroupNum = myHwin.GetOutGroup();
            if (iGroupNum >= 0)
            {

                List<string> lsTemp = FileAdjSQLite.ReadHistory(iGroupNum);
                lbFileNames.Items.Clear();
                rtbStatus.Document.Blocks.Clear();

                foreach (string s in lsTemp)
                {
                    string[] strTmp = s.Split('|');
                    tbExt.Text = strTmp[1];
                    lbFileNames.Items.Add(strTmp[0]);
                }
                string sTemp = $"Files list to restore file(s) in group {iGroupNum}";
                log.Debug(sTemp);
                rtbStatus.AppendText(sTemp + "\r\n");
            }
            myHwin.Close();
        }

        private void LbFileNames_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetOutFile();
        }

        #endregion

        #region Output File and Naming Section

        private Int64 CollectChecks()
        {
            _eChecked Ireturn = 0;
            if (cbxCombineFiles.IsChecked == true) Ireturn |= _eChecked.CombineFile;
            if (cbxFileHeaders.IsChecked == true) Ireturn |= _eChecked.Headers;
            if (cbxNoBracket.IsChecked == true) Ireturn |= _eChecked.NoBracket;
            if (cbxNoDate.IsChecked == true) Ireturn |= _eChecked.NoDate;
            if (cbxNoSecond.IsChecked == true) Ireturn |= _eChecked.NoSecond;
            if (cbxNoBlankLines.IsChecked == true) Ireturn |= _eChecked.NoBlank;
            if (cbxNumber.IsChecked == true) Ireturn |= _eChecked.UseNum;
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
            if ((InCheck & _eChecked.UseNum) != 0) cbxNumber.IsChecked = true;
            else cbxNumber.IsChecked = false;
        }

        /// <summary>
        /// This function populates the output file name, or clears is if there is no in files.
        /// File name is the root directory of first file in list, and file name -0 and extension choice
        /// or first word of top comment. Lastly the file name is sent to next name to increment
        /// numbr if needed, then set in outfile field
        /// </summary>
        private void SetOutFile()
        {
            // clear name if no in file
            if (lbFileNames.Items.Count < 1)
            {
                tbOutFile.Text = "";
                return;
            }
            // find directory and non-comment name
            string strTempDir, strTempFile, strTemp;
            strTemp = lbFileNames.Items[0].ToString();
            strTempDir = System.IO.Path.GetDirectoryName(strTemp);
            strTempFile = System.IO.Path.GetFileNameWithoutExtension(strTemp);
            // if comment is set find first word in comment or "null"
            if (cbxComment.IsChecked == true)
            {
                DataRow drow = MyDtable.Rows[0];
                strTemp = drow.Field<string>("Parameter1");
                int iTempSpacePosition = 0;
                // zero length you can index
                if (strTemp.Length>0) iTempSpacePosition = strTemp.IndexOf(' ',1);
                // if didn't find space one character deep determine if any word is there
                if(iTempSpacePosition < 1)
                {
                    if (strTemp.Length < 2) strTempFile = "null";
                    else strTempFile = strTemp;
                }
                else // found space in phrase
                {
                    strTempFile = strTemp.Substring(0, iTempSpacePosition);
                }
                // need to clean file names characters
                foreach(char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    strTempFile=strTempFile.Replace(c, '_');
                }
            }
            // form out file from pieces
            strTemp = strTempDir + "\\" + strTempFile + "-0" + tbExt.Text;
            // find next available file
            tbOutFile.Text = NextFreeFilename(strTemp);
        }

        /// <summary>
        /// This checks if out file name exists and if it does increaments the -number at the end
        /// </summary>
        /// <param name="inStr">
        /// Standard out file name used by this program directory/filename-number.extension
        /// </param>
        /// <returns></returns>
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

        /// <summary>
        /// This function clears the status windows and then adds sent text, if logging is set to true it also
        /// logs the message
        /// </summary>
        /// <param name="inStr"></param>
        /// <param name="blLog"></param>
        private void ClearStatusAndShow(string inStr, bool blLog = false)
        {
            rtbStatus.Document.Blocks.Clear();
            rtbStatus.AppendText(inStr);
            if (blLog) log.Info(inStr);
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
                    myRow["Group"] = lTemp;
            }
            blUsingActionsHistory = false;
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
                        if((I64_eChecked & (Int64)_eChecked.UseNum) != 0)
                        {
                            if (iNumAdditionLinesAfterInclude > 0)
                            {
                                iNumAdditionLinesAfterInclude--;
                                return true;
                            }
                        }
                        // No bracket check
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
                                iNumAdditionLinesAfterInclude = iNumLimit;
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
                                iNumAdditionLinesAfterInclude = iNumLimit;
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
                                    iNumAdditionLinesAfterInclude = iNumLimit;
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

        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            ActionRowData arowdata = new ActionRowData();
            arowdata.ActionType = strLastActionType;
            arowdata.Param1 = "";
            arowdata.Param2 = "";

            arowdata = WinEditParams.GetEdit(arowdata, "Please enter values for new row or cancel to exit");
            if (arowdata != null)
            {
                strLastActionType = arowdata.ActionType;
                
                blUsingActionsHistory = false;

                // adding new row to bottom of the table
                int i = MyDtable.Rows.Count;
                DataRow drow = MyDtable.Rows[--i];
                Int64 iOrder = drow.Field<Int64>("Order");
                // need to get last order number to increase by one
                MyDtable.Rows.Add(++iOrder, drow.Field<Int64>("Group"),
                    strLastActionType, arowdata.Param1, arowdata.Param2);
            }
        }

        private void BtnClearRows_Click(object sender, RoutedEventArgs e)
        {
            GetString myGet = new GetString("Enter Comment String","A new set of actions start with a comment");
            if(myGet.ShowDialog()==true)
            {
                blUsingActionsHistory = false;
                MyDtable.Rows.Clear();
                Int64 i = FileAdjSQLite.GetActionint();
                string strCommet1Param = myGet.GetAnswer();
                MyDtable.Rows.Add(1, ++i, "Comment", strCommet1Param, "");
                ClearStatusAndShow($"Created new action starting with {strCommet1Param}", true);
                SetOutFile();
            }
        }

        private void BtnSavePreset_Click(object sender, RoutedEventArgs e)
        {
            if (!blUsingActionsHistory) SaveHistory();
            I64_eChecked = CollectChecks();
            Int64 iGroup = MyDtable.Rows[0].Field<Int64>(1);
            SavePreset mySavePreset = new SavePreset(iGroup,I64_eChecked,iNumLimit);
            if (mySavePreset.ShowDialog() == true)
            {
                string sTemp = $"Saved preset for group {iGroup}";
                rtbStatus.AppendText($"{sTemp}\r\n");
                log.Info(sTemp);
            }
        }

        private void BtnLoadPreset_Click(object sender, RoutedEventArgs e)
        {
            FindPreset MyFindPreset = new FindPreset(true);
            if (MyFindPreset.ShowDialog() == true)
            {
                Int64 iTemp = MyFindPreset.GetGroup();
                if (iTemp >= 0)
                {
                    MyDtable = GetTable(iTemp);
                    dgActions.DataContext = MyDtable.DefaultView;
                }
                Int64 iTemp2 = FileAdjSQLite.GetPresetFlags(iTemp);
                SetChecks((_eChecked)iTemp2);
                iNumLimit = (int)( iTemp2 >> 7);
                if (!(iNumLimit > 0 && iNumLimit < 51)) iNumLimit = 0;
                    SldRows_Load();
            }
        }

        private void BtnSwapAbove_Click(object sender, RoutedEventArgs e)
        {
            SwapRow(dgActions.SelectedIndex);
        }

        private void SwapRow(int iSelectedRow)
        {
            if (iSelectedRow < 1)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("You have to select a row below top row.\r\n(You should ctrl-click on row to swap.)",
                    "Can't swap rows",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                DataTable dtParent = MyDtable;
                DataRow row = MyDtable.Rows[iSelectedRow];
                DataRow newRow = dtParent.NewRow();
                newRow.ItemArray = row.ItemArray;
                int iOrderNew = int.Parse(newRow["Order"].ToString()) - 1;
                newRow["Order"] = iOrderNew;
                if (iSelectedRow > 0 && iSelectedRow <= dtParent.Rows.Count)
                {
                    dtParent.Rows.Remove(row);
                    dtParent.Rows.InsertAt(newRow, iSelectedRow - 1);
                    MyDtable.Rows[iSelectedRow]["Order"] = iOrderNew + 1;
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

        private void btnModifyPresets_Click(object sender, RoutedEventArgs e)
        {
            ModifyPreset myMod = new ModifyPreset();
            if (myMod.ShowDialog() == true)
            {
                string strDone = myMod.strResult;
                log.Info(strDone);
                rtbStatus.AppendText($"{strDone}\r\n");
            }

        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // Testing for Multiple Files and Comment checked and not Append Files Selected
            if ((lbFileNames.Items.Count > 1) && (cbxComment.IsChecked == true) && (cbxCombineFiles.IsChecked == false))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Program can't process, either uncheck Comment or check Combine Files, please.",
                    "Multiple files with comments and no combine",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Issue 54 testing for no files and issuing warning
            if (lbFileNames.Items.Count < 1)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("You haven't selected any files to process in upper left section.",
                    "Can't start processing files",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // cblines stores the number of lines in format <num of lines>space some other text
            string strTemp = cbLines.SelectedValue.ToString();
            string[] words = strTemp.Split(' ');
            if (!Int64.TryParse(words[0], out lLinesPerFile))
            {
                log.Error("Error: Using default lines couldn't parse number of lines.");
                lLinesPerFile = 10000;
            }
            log.Debug($"Start button with line limit of {lLinesPerFile} ");
            btnCancel.IsEnabled = true;
            btnOpenNotePad.IsEnabled = false;
            btnStart.IsEnabled = false;
            // removing this logic and saving files every time
            //if (!blUsingFileHist)
            //{
            Int64 iTemp = FileAdjSQLite.GetFileHistoryInt();
            iTemp++;
            rtbStatus.AppendText($"Saving File History #{iTemp}\r\n");
            for (int i = 0; i < lbFileNames.Items.Count; i++)
            {
                FileAdjSQLite.WriteFileHistory(iTemp, lbFileNames.Items[i].ToString(), tbExt.Text);
            }
            //}
            if (!blUsingActionsHistory) SaveHistory();
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

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            MyWorker.CancelAsync();
        }

        private void BtnOut2In_Click(object sender, RoutedEventArgs e)
        {
            ClearFiles();
            AddFile(strFileOut);
        }

        private void CbxComment_Checked(object sender, RoutedEventArgs e)
        {
            SetOutFile();
        }

        private void CbxComment_Unchecked(object sender, RoutedEventArgs e)
        {
            SetOutFile();
        }

        private void BtnInc_Click(object sender, RoutedEventArgs e)
        {
            // find next available file
            string strTemp = NextFreeFilename(tbOutFile.Text);
            if(strTemp == tbOutFile.Text)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Didn't increament the outfile.",
                                "There isn't a file with current out file name.", MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
            }
            else
            {
                tbOutFile.Text = strTemp;
            }

        }

        /// <summary>
        /// Cleans up all files that match output file name in the directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCleanUp_Click(object sender, RoutedEventArgs e)
        {
            // checking to see if tbOutfile text is null or short
            string strTemp = tbOutFile.Text;
            if(string.IsNullOrEmpty(strTemp)||strTemp.Length<11)
            {
                ClearStatusAndShow("Output file string is null or too short to clean up files");
                return;
            }
            string strTempDir, strTempFile, strTempExt;
            strTempDir = System.IO.Path.GetDirectoryName(strTemp);
            strTempFile = System.IO.Path.GetFileNameWithoutExtension(strTemp);
            // This was crasshing without '-' as part of the name, like using asrun option
            int iIndex = strTempFile.LastIndexOf('-');
            if (iIndex > 0)
            {
                strTempFile = strTempFile.Substring(0,iIndex);
                strTempExt = System.IO.Path.GetExtension(strTemp);
                int iCount = 0;
                var dir = new DirectoryInfo(strTempDir);

                foreach (var file in dir.EnumerateFiles(strTempFile + "-*" + strTempExt))
                {
                    iCount++;
                    file.Delete();
                }
                ClearStatusAndShow($"Deleted {iCount} files in {strTempDir} matching pattern {strTempFile}-*{strTempExt}", true);
            } else // We might have found cleanup for asrun
            {
                if(strTempFile== "on_air_temp")
                {
                    if (File.Exists(strTemp)) File.Delete(strTemp);
                    rtbStatus.AppendText($"Deleted file {strTemp}\r\n");
                    strTempDir = strTempDir + @"\OnAir";
                    if (Directory.Exists(strTempDir))
                    {
                        Directory.Delete(strTempDir, true);
                        rtbStatus.AppendText($"Deleted directory {strTempDir}\r\n");
                    } else
                    ClearStatusAndShow($"Could not find to directory {strTempDir} to delete");
                }
            }
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

        #endregion

        #region External, Lines and Status Bars Section

        private void BtnOpenNotePad_Click(object sender, RoutedEventArgs e)
        {
            Process myProcess = new Process();
            try
            {
                Process.Start("notepad++.exe", $"\"{strFileOut}\"");
            }
            catch
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error",
                    "Tried to open " + strFileOut + " in Notepad++.exe but failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                log.Error($"Failed Notepad++ open of {strFileOut}");
            }
        }

        private void BtnOpenDir_Click(object sender, RoutedEventArgs e)
        {
            if (tbOutFile.Text.Length > 1)
            {
                string strDir = System.IO.Path.GetDirectoryName(tbOutFile.Text);
                if (strDir.Length > 1)
                {
                    if (Directory.Exists(strDir))
                    {
                        System.Diagnostics.Process.Start(strDir);
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("Directory no longer exists",
                            "Can't open directory",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Program didn't parse directory from output file.",
                        "Can't open directory",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(
                    "This button is linked to output file, and it is empty.", "Can't open directory",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
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

        #endregion

        #region Actions and Controls for Actions Section

        private void BtnEditRow_Click(object sender, RoutedEventArgs e)
        {
            int iTemp = dgActions.SelectedIndex;
            if (iTemp > -1 && iTemp < MyDtable.Rows.Count)
            {
                ActionRowData arowdata = new ActionRowData();
                arowdata.ActionType = MyDtable.Rows[iTemp].Field<string>("Action");
                arowdata.Param1 = MyDtable.Rows[iTemp].Field<string>("Parameter1");
                arowdata.Param2 = MyDtable.Rows[iTemp].Field<string>("Parameter2");

                arowdata = WinEditParams.GetEdit(arowdata, "Please Edit Parameters or cancel this dialog");
                if (arowdata != null)
                {
                    MyDtable.Rows[iTemp][2] = arowdata.ActionType;
                    MyDtable.Rows[iTemp][3] = arowdata.Param1;
                    MyDtable.Rows[iTemp][4] = arowdata.Param2;
                }
            }
            else Xceed.Wpf.Toolkit.MessageBox.Show("You have to select a valid Action Group row to edit.", 
                "Operational Hint-Left click on row",MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
        }

        private void RtbStatus_TextChanged(object sender, TextChangedEventArgs e)
        {
            rtbStatus.ScrollToEnd();
        }

        private void BtnHistAction_Click(object sender, RoutedEventArgs e)
        {
            HIstoryWin m_Hwin = new HIstoryWin(true);
            if ((bool)m_Hwin.ShowDialog())
            {
                Int64 iGroupNum = m_Hwin.GetOutGroup();
                if (iGroupNum >= 0)
                {
                    MyDtable = GetTable(m_Hwin.GetOutGroup());
                    dgActions.DataContext = MyDtable.DefaultView;
                    string sTemp = $"History list to restore actions in group {iGroupNum}";
                    log.Debug(sTemp);
                    rtbStatus.AppendText(sTemp + "\r\n");
                    blUsingActionsHistory = true;
                }
            }
            m_Hwin.Close();
        }

        private void BtnQuickAddIn_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                string clipboardText = Clipboard.GetText(TextDataFormat.Text);
                clipboardText = clipboardText.Trim(' ');
                MyDtable.Rows.Clear();
                Int64 i = FileAdjSQLite.GetActionint();
                MyDtable.Rows.Add(1, ++i, "Comment", clipboardText, "");
                ClearStatusAndShow($"Created new action starting with {clipboardText} trimmed", true);
                QuickAddRow("Include", clipboardText);
                SetOutFile();
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Can't read clipboard",
                    "Can't Add Action and Include Row from clipboard text",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

        }

        private void SldRows_Load()
        {
            lbNumRows.Content = iNumLimit.ToString("00");
            sldRows.Value = iNumLimit;
        }

        private void DgActions_CurrentCellChanged_1(object sender, EventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // it seems that with the CurrentCellChanged event, the value of SelectedIndex always displays the previously selected index
                SwapRow((int)dgActions.Items.IndexOf(dgActions.CurrentItem));
            }
        }

        private void BtnOnAir_Click(object sender, RoutedEventArgs e)
        {
            // Testing for no files and issuing warning
            if (lbFileNames.Items.Count < 1)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("You haven't selected any files to process in upper left section.",
                    "Can't start processing files",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Set out file name, delete file if exists
            string strTemp = lbFileNames.Items[0].ToString();
            string strTempDir = System.IO.Path.GetDirectoryName(strTemp);
            strTemp = strTempDir + @"\on_air_temp.txt";
            if (File.Exists(strTemp)) File.Delete(strTemp);
            strFileOut = strTemp;
            Int64 iTemp = FileAdjSQLite.GetOnAirAction();
            log.Debug($"Found {iTemp} preset group");
            MyDtable = GetTable(iTemp);
            dgActions.DataContext = MyDtable.DefaultView;
            // special mode for on air work
            blUsingOnAirMode = true; 
            // increasing lines to hold everything in one file, and changing window statuses
            lLinesPerFile = 1000000;
            btnCancel.IsEnabled = true;
            btnOpenNotePad.IsEnabled = false;
            btnStart.IsEnabled = false;
            cbxFileHeaders.IsChecked = false;
            strTemp = $"\r\nStarting As Run breakout on top file in list to {strFileOut}\r\n";
            LogAndAppend(strTemp);
            List<string> lFileList = new List<string>();
            lFileList.Add(lbFileNames.Items[0].ToString());
            MyWorker.RunWorkerAsync(lFileList);
        }

        private void SldRows_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            iNumLimit = (int)sldRows.Value;
            SldRows_Load();
        }

        private void BtnDelRow_Click(object sender, RoutedEventArgs e)
        {
            if (MyDtable.Rows.Count < 2)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Sorry you can't delete last row.",
                    "Operational Training",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
            else
            {
                int iTemp = dgActions.SelectedIndex;
                if (iTemp > 0 && iTemp < MyDtable.Rows.Count)
                {
                    MyDtable.Rows[iTemp].Delete();
                }
                else Xceed.Wpf.Toolkit.MessageBox.Show(
                    "You have to select a valid row to delete and you can't delete the top row. ", 
                    "Operational Hint-Left click on row", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        #endregion

        #region Worker Thread

        private void MyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> lInFiles = (List<string>)e.Argument;
            long lNumOfLines = 0;
            bool blWorkingInsideFileList = false;
            long lNumFiles = lInFiles.Count;
            long lCurNumFile = 0, lCurBytesRead = 0, lNullsInFile = 0;
            byte[] inbuffer = new byte[16 * 4096];
            byte[] outbuffer = new byte[17 * 4096]; // Allowing for extended string buffer
            byte[] strbuffer = new byte[2003];
            int read, icount, iStrLen = 0;
            int iOut = 0;
            bool blHitLastLine = false;
            bool blLineOverRun = false;
            // First Entry in my Rport is for non-existing file errors
            myRport.Add(new jobReport { filename = "", error = "", lines = 0 });


            foreach (string sInFile in lInFiles)
            {
                if (File.Exists(sInFile))
                {
                    input = File.Open(sInFile, FileMode.Open);
                    lCurNumFile++;
                    myRport.Add(new jobReport { filename = sInFile, error = "", lines = 0 });
                    lNullsInFile = 0;
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
                                        myRport[(int)lCurNumFile].lines++;
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
                                lNullsInFile++;
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
                    // on closing input file write error
                    myRport[(int)lCurNumFile].error = $"Nulls: {lNullsInFile}";
                    if (MyWorker.CancellationPending) e.Cancel = true;
                }
                else  //file didn't exist
                {
                    myRport[0].filename = "Error missing file(s):";
                    myRport[0].error += " " + sInFile + " ";
                    myRport[0].lines++;
                }// end looping output files// end of input files
                MyWorker.ReportProgress((int)(((double)lCurBytesRead / (double)lFileSize) * 100.0) +
    (int)(((double)lCurNumFile / (double)lNumFiles) * 100000.0));
            }
            if (output != null) output.Close();  // closing combined files or if missed check above
        }

        private void MyWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int i = e.ProgressPercentage;
            //log.Debug($"Progress of {i}");
            int ii = i % 100;
            pbProgress.Value = ii;
            if (i > 1000)
                pbFiles.Value = (i - ii) / 1000;
            else pbFiles.Value = 0;
        }

        private void MyWorker_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            pbProgress.Value = 0;
            pbFiles.Value = 0;
            tbOutFile.Text = strFileOut;
            btnStart.IsEnabled = true;
            btnOpenNotePad.IsEnabled = true;
            btnCancel.IsEnabled = false;
            if (iCountOfNulls > 0) rtbStatus.AppendText($"Found {iCountOfNulls} null characters in files which weren't copied");
            foreach (jobReport jR in myRport)
            {
                // the report starts with 0 entry for skipped files that has empty filename if no error
                if(jR.filename.Length>2)LogAndAppend($"  lines written: {jR.lines}\t{jR.filename}\t{jR.error}");
            }
            myRport.Clear();
            if (blUsingOnAirMode) OnAirLogSplitting();
            else
            {
                TimeSpan mySpan = DateTime.Now - myStartTime;
                LogAndAppend($"{DateTime.Now.TimeOfDay} Complete in {mySpan.Seconds} seconds, last {tbOutFile.Text}");
            }
        }

        #endregion

        #region Logging and Rich Text Box Section

        /// <summary>
        /// On air file broken in to sub text files
        /// </summary>
        private void OnAirLogSplitting()
        {
            ParseOnAir myOnAir = new ParseOnAir(strFileOut);
            myOnAir.Show();
            string strDirPath = myOnAir.getDir();
            blUsingOnAirMode = false;
        }

        private void LogAndAppend(string strIn)
        {
            log.Info(strIn);
            rtbStatus.AppendText(strIn+"\r\n");
        }

        #endregion
    }
}
