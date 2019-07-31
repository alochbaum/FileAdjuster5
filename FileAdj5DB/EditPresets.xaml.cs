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
        private List<CPreset> myLCP = new List<CPreset>();
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

        private void BtnEditName_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayGrid.SelectedIndex == -1)
                MessageBox.Show("You have to select row", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                CPreset PresetRow = (CPreset)DisplayGrid.SelectedItems[0];
                MessageBox.Show(PresetRow.Name);

            }
        }

        /// <summary>
        /// This loads Display Grid with Presets from Database String
        /// </summary>
        /// <param name="strDB">Full filepath of database</param>
        private void DisplayGridFromDB(string strDB)
        {
            myLCP = mySQL.GetCPresets(inTdb);
            myDT = myLCP.ToDataTable<CPreset>();
            DisplayGrid.ItemsSource = myLCP;
            DisplayGrid.DataContext = myDT.DefaultView;
        }
    }
}
