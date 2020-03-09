using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
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
    /// Interaction logic for FindPreset.xaml
    /// </summary>
    public partial class FindPreset : Window
    {
        private DataTable MyDTPresets = new DataTable();
        private Int64 iGroup = -1;
        private Int64 iFlag = 3;
        private bool blSelecting = true;
        private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FindPreset(bool isSelecting)
        {
            InitializeComponent();
            LoadPresetTable();
            if (!isSelecting)
            {
                blSelecting = false;
                PresetWin.Title = "Select preset(s). Note: all presets with same name get deleted if you select just one.";
                lbinfo.Content="<- Click button to delete or other button to exit ->";
                btnDelete.Visibility = System.Windows.Visibility.Visible;
                btnDelete.IsEnabled = true;
            }
        }

        private void LoadPresetTable()
        {
            MyDTPresets = FileAdjSQLite.ReadPresets();
            log.Debug($"Read Preset Row 1 Column 1 is {MyDTPresets.Rows[0].Field<string>(1)}");
            dgPresets.DataContext = MyDTPresets.DefaultView;
            dgPresets.UnselectAll();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = (DataRowView)dgPresets.SelectedItem;
            String result = (drv["Group"]).ToString();
            if (Int64.TryParse(result, out long iTemp)) iGroup = iTemp;
            log.Debug($"Clicked OK with {iGroup.ToString()}");
            this.DialogResult = true;
        }
        public Int64 GetGroup()
        {
            return iGroup;
        }
        /// <summary>
        /// This finds selected preset group and changes its priority with the group above
        /// </summary>
        private void ShiftGroups()
        {
            DataRowView drv = (DataRowView)dgPresets.SelectedItem;
            if (drv != null)
            {
                String result = (drv["Preset Group"]).ToString();
                log.Debug($"Set to move up {result}");
                FileAdjSQLite.ShiftPresetUp(result);
                LoadPresetTable();
            }
        }

        private void DgPresets_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                BtnOK_Click(this, new System.Windows.RoutedEventArgs());
            else ShiftGroups();
        }
    }
}
