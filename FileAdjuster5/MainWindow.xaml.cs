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
            lFileSize = 0, lLinesPerFile = 0, lLastHistory =0;
        // This holds current out file
        private string strFileOut="";
        private BackgroundWorker MyWorker;
        private string strRTB = "";
        private bool blPosNum = true, blShowChar = true;

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
            // Get the DataTable.
            DataTable MyDtable = GetTable();
            dgActions.DataContext = MyDtable.DefaultView;

        }
        static DataTable GetTable()
        {
            // Here we create a DataTable with four columns.
            DataTable table = new DataTable();
            table.Columns.Add("Order", typeof(int));
            table.Columns.Add("Action", typeof(string));
            table.Columns.Add("Parameter1", typeof(string));
            table.Columns.Add("Parameter2", typeof(string));

            // Here we add five DataRows.
            table.Rows.Add(1, "Exclude", "XMedia", "");
            table.Rows.Add(2, "Only Include", "TXPlay02", "On Air");
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
            // cblines stores the number of lines to work 
            string strTemp = cbLines.SelectedValue.ToString();
            string[] words = strTemp.Split(' ');
            if(!Int64.TryParse(words[0],out lLinesPerFile))
            {
                // Just for testing
                lLinesPerFile = 10000;
            }
            btnCancel.IsEnabled = true;
            btnStart.IsEnabled = false;
            Int64 iTemp = FileAdjSQLite.GetHistoryint();
            iTemp++;
            for (int i = 0; i < lbFileNames.Items.Count; i++)
            {
                FileAdjSQLite.WriteHistory(iTemp, lbFileNames.Items[i].ToString());
            }
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
                tbOutFile.Text = baseDir + "\\" + strFile + System.IO.Path.GetExtension(strTemp);
            } else
            {
                tbOutFile.Text = "";
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            lLastHistory = 0;
            lbFileNames.Items.Clear();
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            if (lLastHistory < 1) lLastHistory = FileAdjSQLite.GetHistoryint();
            else lLastHistory--;
            List<string> lsTemp = FileAdjSQLite.ReadHistory(lLastHistory);
            lbFileNames.Items.Clear();
            foreach(string s in lsTemp)
            {
                lbFileNames.Items.Add(s);
            }
        }

        private void Btn_Fix_Click(object sender, RoutedEventArgs e)
        {
            List<string> lsSizes = new List<string>
            {
                "555000 Std",
                "55500 Small",
                "500000 Med"
            };
            cbLines.ItemsSource = lsSizes;

            // ... Make the first item selected.
            cbLines.SelectedIndex = 0;
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
            byte[] outbuffer = new byte[16 * 4096];
            string strHoldLast50 = "";
            int read, icount;
            long lStoredPosition = 0;
            bool blHitLastLine = false;
            // looping the full file size
            while ((read = input.Read(inbuffer, 0, inbuffer.Length)) > 0)
            {
                int iOut = 0, iicount = 0;
                    // looping the input buffer
                    for (icount = iicount; icount < read; icount++, iicount++)
                    {

                    if (inbuffer[icount] != 0)
                    {
                        outbuffer[iOut] = inbuffer[icount];
                        iOut++;
                        if (inbuffer[icount] == '\r')
                        {
                            lNumOfLines++;
                            if (lNumOfLines >= lLinesPerFile)
                            {
                                blHitLastLine = true;
                                //MessageBox.Show("Hit it");
                            }
                        }
                    }
                    else
                    {
                        if (lPosition == lStoredPosition)
                            lNullsNum++;
                        else
                        {
                            if (lStoredPosition > 0 || lNullsNum > 0)
                            {
                                strRTB = strHoldLast50;
                                if (blPosNum)
                                {
                                    string s1 = $"Position {lStoredPosition} has {lNullsNum} null characters";
                                    strRTB += s1 + "\r\n";
                                    string s2 = new string('-', s1.Length);
                                    strRTB += s2 + "\r\n";
                                }
                                lNullsNum = -1;
                            }
                            lNullsNum++;
                        }
                        lStoredPosition = lPosition;


                    } // end looping output not hit
                      // looping output files, I'm thinking this has to be inside looping input buffer
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
                    if (blShowChar)
                    {
                        int iTemp = 255;
                        if (iOut < iTemp) iTemp = iOut;
                        var bSmall = new Byte[iTemp];
                        bSmall = outbuffer.Skip(iOut - iTemp).Take(iTemp).ToArray();
                        var stream = new StreamReader(new MemoryStream(bSmall));
                        strHoldLast50 = stream.ReadToEnd();
                    }
                    iicount = 0;
                    lPosition += read;
                    // checking to see if user click cancel, if they did get out of loop
                    if (MyWorker.CancellationPending) break;
                    MyWorker.ReportProgress((int)(((double)lPosition / (double)lFileSize) * 100.0));
                } // end looping output files
                else
                {
                    // finish writing out block plus one character for \n
                    if (iOut > 0)
                    {
                        output.Write(outbuffer, 0, iOut);
                    }
                } // end looping full file size
            }
            output.Close();
            input.Close();
            if (MyWorker.CancellationPending) e.Cancel = true;
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
                + System.IO.Path.GetExtension(inStr);
            return strOut;
        }
        void MyWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbProgress.Value = e.ProgressPercentage;
        }
        void MyWorker_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            pbProgress.Value = 0;
            btnStart.IsEnabled = true;
            btnCancel.IsEnabled = false;
        }
    
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            MyWorker.CancelAsync();
        }
    }
}
