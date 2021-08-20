using System.ComponentModel;
using System.Windows;
using System.Windows.Shapes;
using System.IO;
using System.Collections.Generic;
using System;

namespace FileAdjuster5
{
    /// <summary>
    /// Creating new ParseOnAir Window, so I can easily report progress of scan
    /// </summary>
    ///
    public partial class ParseOnAir : Window
    {
        private BackgroundWorker MyWorker2;
        private string strDirPath = "",strFileOut, strEnd;
        private List<string> lChannels = new List<string>();
        private Int64 icount = 0,iTotal;
        /// <summary>
        /// Creation
        /// </summary>
        /// <param name="strFile2Parse">This is single file to please with search parameters</param>
        public ParseOnAir(string strFile2Parse)
        {
            InitializeComponent();
            strFileOut = strFile2Parse;
            MyWorker2 = (BackgroundWorker)this.FindResource("MyWorker2");
            beforeDoWork(strFile2Parse);
            MyWorker2.RunWorkerAsync();

        }
        public string getDir() { return strDirPath; } 
        private void beforeDoWork(string strFile)
        {
            strDirPath = System.IO.Path.GetDirectoryName(strFile) + @"\OnAir";
            if (Directory.Exists(strDirPath))
            { Directory.Delete(strDirPath, true); }
            Directory.CreateDirectory(strDirPath);
        }
        private void OnAirLog_DoWork(object sender, DoWorkEventArgs e)
        {
            int ipos = 0, ipos2 = 0;
            // Read the file and display it line by line.  
            var file = File.ReadAllLines(strFileOut);
            var strlines = new List<string>(file);
            // report progress on background thread
            iTotal = strlines.Count;
            foreach (string strline in strlines)
            {
                ipos = strline.IndexOf('|');
                if (ipos > 1)
                {
                    ipos2 = strline.IndexOf(':', ipos);
                    strEnd = strline.Substring(ipos + 1, ipos2 - ipos - 1);
                    ipos2 = strEnd.LastIndexOf('-');
                    if (ipos2 > 0)
                    {
                        strEnd = strEnd.Substring(0, ipos2);
                        if (!lChannels.Contains(strEnd))
                        {
                            lChannels.Add(strEnd);
                        }
                        using (FileStream fs = new FileStream(strDirPath + @"\" + strEnd + ".txt",
                            FileMode.Append, FileAccess.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                sw.WriteLine(strline);
                            }
                        }
                    }
                }

                icount++;
                MyWorker2.ReportProgress(1);
            }

        }
        private void OnAirLog_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            float fTemp = ((float)icount / (float)iTotal)*100;
            lblStatus.Content = $"Log Line: {icount} Channel: {strEnd}";
            pbAmount.Value = (int)fTemp;
        }
        private void OnAirLog_Complete(object sender, RunWorkerCompletedEventArgs e) { }
    }
}
