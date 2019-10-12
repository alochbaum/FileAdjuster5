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
        private string inTdb, inIdb;
 
        public MovePresets(string inTargetDB, string inInputDB)
        {
            inTdb = inTargetDB;
            inIdb = inInputDB;
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayMoveGridFromDB(inTdb);
        }

        private void DisplayMoveGridFromDB(string inTdb)
        {

        }
    }
}
