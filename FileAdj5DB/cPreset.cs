using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileAdj5DB
{
    public class CPresetType
    {
        public Int64 iId { get; set; }
        public string Name { get; set; }
    }
    public class CDisplayPreset
    {
        public int PresetID { get; set; }
        public string PresetTypeName { get; set; }
        public string PresetName { get; set; }
    }
}
