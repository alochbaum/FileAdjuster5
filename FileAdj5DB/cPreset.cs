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
        public Int64 PresetID { get; set; }
        public string PresetTypeName { get; set; }
        public string PresetName { get; set; }
        public string Date { get; set; }
    }
    public class CAction
    {
        public Int64 DisplayOrder { get; set; }
        public Int64 GroupID { get; set; }
        public Int64 ActionTypeID { get; set; }
        public string Parameter1 { get; set; }
        public string Parameter2 { get; set; }
        public string DateAdded { get; set; }

    }
    public class CPreset
    {
        public Int64 PTypeID { get; set; }
        public string PresetName { get; set; }
        public Int64 GroupID { get; set; }
        public Int64 Flags { get; set; }
        public string DateAdded { get; set; }
    }

}
