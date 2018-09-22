using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileAdjuster5
{
 
    static class ReadLastFromLog
    {
        private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //public List<string> ReadLastLine (int iNum)
        //{
        //     List<string> lreturn = new List<string>();
        //    int n = 5; //or any arbitrary number
        //    int count = 0;
        //    string content;
        //    byte[] buffer = new byte[1];

        //    using (FileStream fs = new FileStream("text.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        //    {
        //        // read to the end.
        //        fs.Seek(0, SeekOrigin.End);

        //        // read backwards 'n' lines
        //        while (count < n)
        //        {
        //            fs.Seek(-1, SeekOrigin.Current);
        //            fs.Read(buffer, 0, 1);
        //            if (buffer[0] == '\n')
        //            {
        //                count++;
        //            }

        //            fs.Seek(-1, SeekOrigin.Current); // fs.Read(...) advances the position, so we need to go back again
        //        }
        //        fs.Seek(1, SeekOrigin.Current); // go past the last '\n'

        //        // read the last n lines
        //        using (StreamReader sr = new StreamReader(fs))
        //        {
        //            content = sr.ReadToEnd();
        //        }
        //    }
        //    return lreturn;

        //}
    }
}
