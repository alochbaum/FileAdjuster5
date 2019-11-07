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

namespace FileAdj5DB
{
    /// <summary>
    /// Interaction logic for MovePresets.xaml
    /// </summary>
    public partial class MovePresets : Window
    {
        private DataTable myDT = new DataTable();
        private List<CDisplayPreset> myLDP = new List<CDisplayPreset>();
        private SqLiteModifer mySQL = new SqLiteModifer();
        private string inTdb, inSdb;
 
        public MovePresets(string inTargetDB, string inSourceDB)
        {
            inTdb = inTargetDB;
            inSdb = inSourceDB;
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayMoveGridFromDB(inSdb);
        }

        private void DGMoveGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        private void BtnMove_Click(object sender, RoutedEventArgs e)
        {
            if (DGMoveGrid.SelectedValue!=null)
            {
                CDisplayPreset myDP = (CDisplayPreset)DGMoveGrid.SelectedItems[0];
                // Getting PresetID for saving preset in target with that ID
                Int64 iPresetID = mySQL.GetIsPresetType(inTdb, myDP.PresetTypeName);
                if (iPresetID >= 0)
                {
                    string strResult = mySQL.MovePreset(inSdb, inTdb, myDP.PresetName,
                        iPresetID);
                    if (strResult!="Done")
                        MessageBox.Show($"Error {strResult} moving preset");
                    else MessageBox.Show("No errors while moving", "Its Good!");
                }
                else MessageBox.Show("There isn't a Preset Type with that name", "Error in Target DB");
            } else
            {
                MessageBox.Show("You have to select row to move","Please Select Row");
            }
        }

        private void DisplayMoveGridFromDB(string inSdb)
        {
            myLDP = mySQL.GetDisplayPresets(inSdb);
            myDT = myLDP.ToDataTable<CDisplayPreset>();
            DGMoveGrid.ItemsSource = myLDP;
            DGMoveGrid.DataContext = myDT.DefaultView;

        }
    }
}
