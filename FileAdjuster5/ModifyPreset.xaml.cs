using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for ModifyPreset.xaml
    /// </summary>
    public partial class ModifyPreset : Window
    {
        public string strResult { get; set; }
        public ModifyPreset()
        {
            InitializeComponent();
            strResult = "No Action";
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Title = "Enter database file to save presets to",
                Filter = "sqlite|*.sqlite"
            };
            if (dlg.ShowDialog() == true)
            {
                if (File.Exists(dlg.FileName)) File.Delete(dlg.FileName);
                FileAdjSQLite.ExportPresets(dlg.FileName);
                strResult = $"Exported presets to {dlg.FileName}";
            }
            else strResult = "Cancelled export of presets";
            DialogResult = true;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            FindPreset myFind = new FindPreset(false);
            if(myFind.ShowDialog() == true)
            {
                strResult = "Deleted some presets";
            } else
            {
                strResult = "Cancelled deleting some presets";
            }
            DialogResult = true;
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Enter database file to save presets to",
                Filter = "sqlite|*.sqlite"
            };
            if (dlg.ShowDialog() == true)
            {
            }
        }
    }
}
