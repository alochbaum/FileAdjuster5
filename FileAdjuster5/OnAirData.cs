using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileAdjuster5
{
    class OnAirData
    {
        public string PreSetName { get; set; }
        public string OutFileName { get; set; }
        public int IntStartChar { get; set; }
        public int IntGroupChar { get; set; }
        public int IntGroupPos { get; set; }
        public int IntOutChar { get; set; }
        public Int64 LongLinesPerFile { get; set; }

    }
}
