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
    /// Interaction logic for EditPresets.xaml
    /// </summary>
    public partial class EditPresets : Window
    {
        private DataTable myDT = new DataTable();
        private List<CPresetType> myLCP = new List<CPresetType>();
        private SqLiteModifer mySQL = new SqLiteModifer();
        private string inTdb, inIdb;
        public EditPresets(string inTargetDB,string inInputDB)
        {
            inTdb = inTargetDB;
            inIdb = inInputDB;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayGridFromDB(inTdb);
        }

        private void DisplayGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            TextBox objTextBox = (TextBox)e.EditingElement;
            CPresetType PresetRow = (CPresetType)DisplayGrid.SelectedItems[0];
            MessageBox.Show(objTextBox.Text, PresetRow.iId.ToString());
            mySQL.RenamePresetType(inTdb, PresetRow.iId.ToString(), objTextBox.Text);
            //MessageBox.Show(DisplayGrid.)
        }

        /// <summary>
        /// This loads Display Grid with Preset Types from Database String
        /// </summary>
        /// <param name="strDB">Full filepath of database</param>
        private void DisplayGridFromDB(string strDB)
        {
            myLCP = mySQL.GetCPresetsTypes(inTdb);
            myDT = myLCP.ToDataTable<CPresetType>();
            DisplayGrid.ItemsSource = myLCP;
            DisplayGrid.DataContext = myDT.DefaultView;
        }
    }
}
