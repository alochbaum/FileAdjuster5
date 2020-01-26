﻿using System;
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
        private DateTime myStartTime;
        private DataTable MyDtable = new DataTable();
        private List<jobReport> myRport = new List<jobReport>();

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
            // Testing for Multiple Files and Comment checked and not Append Files Selected
            if ((lbFileNames.Items.Count>1)&&(cbxComment.IsChecked == true) && (cbxCombineFiles.IsChecked == false))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Program can't process, either uncheck Comment or check Combine Files, please.",
                    "Multiple files with comments and no combine",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
            string sReport = "Error Reading File History: no files.";  // outputs error if not replaced
            foreach(string s in lsTemp)
            {
                string[] sTemp = s.Split('|');
                tbExt.Text = sTemp[1];
                sReport="Read File History: "+sTemp[0]+" created on "+sTemp[2];
                lbFileNames.Items.Add(sTemp[0]);
            }
            log.Debug(sReport);
            rtbStatus.AppendText(sReport + "\r\n");
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
            long lCurNumFile = 0, lCurBytesRead = 0, lNullsInFile =0;
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
                string strCommet1Param = myGet.GetAnswer();
                MyDtable.Rows.Add(1, ++i, "Comment", strCommet1Param, "");
                ClearStatusAndShow($"Created new action starting with {strCommet1Param}", true);
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
            strTempFile = strTempFile.Substring(0, strTempFile.LastIndexOf('-'));
            strTempExt = System.IO.Path.GetExtension(strTemp);
            int iCount = 0;
            var dir = new DirectoryInfo(strTempDir);
 
            foreach (var file in dir.EnumerateFiles(strTempFile+"-*"+strTempExt))
            {
                iCount++;
                file.Delete();
            }
            ClearStatusAndShow($"Deleted {iCount} files in {strTempDir} matching pattern {strTempFile}-*{strTempExt}", true);
        }

        private void BtnEditParams_Click(object sender, RoutedEventArgs e)
        {
            int iTemp = dgActions.SelectedIndex;
            if (iTemp > -1 && iTemp < MyDtable.Rows.Count)
            {
                TwoParams send2 = new TwoParams();
                send2.Param1 = MyDtable.Rows[iTemp].Field<string>("Parameter1");
                send2.Param2 = MyDtable.Rows[iTemp].Field<string>("Parameter2");
                string strType = MyDtable.Rows[iTemp].Field<string>("Action");
                send2 = WinEditParams.GetEdit(strType, send2);
                if (send2 != null)
                {
                    MyDtable.Rows[iTemp][3] = send2.Param1;
                    MyDtable.Rows[iTemp][4] = send2.Param2;
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

        private void BtbClear_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Clearing Files First
                ClearFiles();
                // The other drop event will add the files
            }
        }

        private void BtnQuickAddIn_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                string clipboardText = Clipboard.GetText(TextDataFormat.Text);
                MyDtable.Rows.Clear();
                Int64 i = FileAdjSQLite.GetActionint();
                MyDtable.Rows.Add(1, ++i, "Comment", clipboardText, "");
                ClearStatusAndShow($"Created new action starting with {clipboardText}", true);
                QuickAddRow("Include", clipboardText);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Can't read clipboard",
                    "Can't Add Action and Include Row from clipboard text",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

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
            foreach (jobReport jR in myRport)
            {
                // the report starts with 0 entry for skipped files that has empty filename if no error
                if(jR.filename.Length>2)LogAndAppend($"  lines written: {jR.lines}\t{jR.filename}\t{jR.error}");
            }
            myRport.Clear();
            TimeSpan mySpan = DateTime.Now - myStartTime;
            LogAndAppend($"{DateTime.Now.TimeOfDay} Complete in {mySpan.Seconds} seconds, last {tbOutFile.Text}");
        }
        private void LogAndAppend(string strIn)
        {
            log.Info(strIn);
            rtbStatus.AppendText(strIn+"\r\n");
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            MyWorker.CancelAsync();
        }
    }
}
