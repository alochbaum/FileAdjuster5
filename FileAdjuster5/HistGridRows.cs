using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileAdjuster5
{
    class HistGridRows
    {
    }
    public class HistActionRows
    {
        public Int64 GroupID { get; set; }
        public string DateAdded { get; set; }
        public string Parameter1 { get; set; }
        public string Parameter2 { get; set; }
    }
    public class HistFileRows
    {
        public Int64 GroupID { get; set; }
        public string DateAdded { get; set; }
        public string FileName { get; set; }
    }
}
