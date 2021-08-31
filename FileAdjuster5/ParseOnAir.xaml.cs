using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System;
using System.Windows.Input;

namespace FileAdjuster5
{
    public class COnAirLine
    {
        public string DFilesName { get; set; }
        public string Line { get; set; }
    }
    /// <summary>
    /// Creating new ParseOnAir Window, so I can easily report progress of scan
    /// </summary>
    ///
    public partial class ParseOnAir : Window
    {
        private BackgroundWorker MyWorker2;
        private string strDirPath = "",strFileOut, strEnd;
        private List<string> lChannels = new List<string>();
        private OnAirData my_OnAirData = new OnAirData();
        private char chStart, chInGroup, chOutGroup;
        private Int64 icount = 0,iTotal,iOffsetInGroup;
        /// <summary>
        /// Creation
        /// </summary>
        /// <param name="strFile2Parse">This is single file to parse with search parameters</param>
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
            my_OnAirData = FileAdjSQLite.ReadOnAirData();
            chStart = (char)my_OnAirData.IntStartChar;
            chInGroup = (char)my_OnAirData.IntGroupChar;
            chOutGroup = (char)my_OnAirData.IntOutChar;
            iOffsetInGroup = my_OnAirData.IntGroupPos;
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
            List<COnAirLine> myOnAir = new List<COnAirLine>();
            iTotal = strlines.Count;
            foreach (string strline in strlines)
            {
                // chStart search is one character to start substring
                ipos = strline.IndexOf(chStart);
                if (ipos > 1)
                {
                    // finding end of group with offset from start
                    ipos2 = strline.IndexOf(chInGroup, ipos+(int)iOffsetInGroup);
                    strEnd = strline.Substring(ipos + 1, ipos2 - ipos - 1);
                    // optional second shortening of group
                    ipos2 = strEnd.LastIndexOf(chOutGroup);
                    if (ipos2 > 0)
                    {
                        strEnd = strEnd.Substring(0, ipos2);
                        if (!lChannels.Contains(strEnd))
                        {
                            lChannels.Add(strEnd);
                        }
                        myOnAir.Add(new COnAirLine() { DFilesName = strEnd, Line = strline });
                    }
                }
                icount++;
                if((icount%25)==0)MyWorker2.ReportProgress(1);
            }
            if (lChannels.Count > 0)
            {
                icount = 0;
                foreach(string channel in lChannels)
                {
                    using (FileStream fs = new FileStream(strDirPath + @"\" + channel + ".txt",
                        FileMode.Append, FileAccess.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            foreach (var oaline in from COnAirLine in myOnAir
                                                   where COnAirLine.DFilesName.CompareTo(channel) == 0
                                                   select COnAirLine)
                            {
                                sw.WriteLine(oaline.Line);
                            }
                        }
                    }
                }
                icount++;
                // I reduced this from 25 out files can be pretty small
                if ((icount % 5) == 0) MyWorker2.ReportProgress(1);
            }
        }
        private void OnAirLog_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int i = e.ProgressPercentage;
            lblStatus.Content = $"Log Line: {icount}";
            float fTemp = ((float)icount / (float)iTotal)*100;
            pbAmount.Value = (int)fTemp;

        }
        private void OnAirLog_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Opening directory if channels were found
            if (lChannels.Count > 0)
            {
                System.Diagnostics.Process.Start(strDirPath);
            }
            else
            {
                Directory.Delete(strDirPath, true);
            }
            this.Hide();
        }
    }
}
