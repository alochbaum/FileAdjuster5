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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileAdj5DB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainDBWindow : Window
    {
        public MainDBWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select File to Add To List (It will not auto start)",
                Filter = "All Files|*.*",
                InitialDirectory = System.IO.Path.GetFullPath(tbSourceDB.Text)
            };

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            if (result != null && result == true)
            {
                tbSourceDB.Text = dlg.FileName;
            }
        }

        private void BtnList_Click(object sender, RoutedEventArgs e)
        {
            SqLiteModifer mySQL = new SqLiteModifer();
            List<CPreset> myList = mySQL.GetCPresets(tbSourceDB.Text);
            foreach (CPreset cp in myList)
            {
                rtbStatus.AppendText($"{cp.iId.ToString()} {cp.Name}\r\n");
            }
        }
    }
}
