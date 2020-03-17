using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FileAdjuster5
{
    public class CDisplayPreset
    {
        public Int64 GroupID { get; set; }
        public string GroupName { get; set; }
        public string PresetName { get; set; }
        public string Date { get; set; }
    }
    /// <summary>
    /// Interaction logic for ImportPreset.xaml
    /// </summary>
    public partial class ImportPreset : Window
    {
        private DataTable MyDTPresets = new DataTable();
        private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string strDBFile = "";
        public ImportPreset(string strDBFileName="")
        {
            InitializeComponent();
            MyDTPresets = FileAdjSQLite.ReadPresets(strDBFileName);
            log.Debug($"Read Preset Row 1 Column 1 is {MyDTPresets.Rows[0].Field<string>(1)}");
            dgImport.DataContext = MyDTPresets.DefaultView;
            dgImport.UnselectAll();
            strDBFile = strDBFileName;
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            List<CDisplayPreset> myLDP = new List<CDisplayPreset>();
            foreach (DataRowView rowView in dgImport.SelectedItems)
            {
                if (rowView != null)
                {
                    DataRow row = rowView.Row;
                    CDisplayPreset DP = new CDisplayPreset();
                    DP.GroupID = int.Parse(row.ItemArray[0].ToString());
                    DP.GroupName = row.ItemArray[1].ToString();
                    DP.PresetName = row.ItemArray[2].ToString();
                    DP.Date = row.ItemArray[3].ToString();
                    myLDP.Add(DP);
                    Int64 iGroup = FileAdjSQLite.GetPresetGroup(DP.GroupName);
                    if (iGroup < 0) FileAdjSQLite.WriteGroup(DP.GroupName);
                    string strVersion = FileAdjSQLite.ReadVersion(strDBFile);
                }

            }
        }
    }
}
