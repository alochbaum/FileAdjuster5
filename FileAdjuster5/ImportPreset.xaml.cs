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
    /// <summary>
    /// Interaction logic for ImportPreset.xaml
    /// </summary>
    public partial class ImportPreset : Window
    {
        private DataTable MyDTPresets = new DataTable();
        private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ImportPreset(string strDBFileName="")
        {
            InitializeComponent();
            MyDTPresets = FileAdjSQLite.ReadPresets(strDBFileName);
            log.Debug($"Read Preset Row 1 Column 1 is {MyDTPresets.Rows[0].Field<string>(1)}");
            dgImport.DataContext = MyDTPresets.DefaultView;
            dgImport.UnselectAll();
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
