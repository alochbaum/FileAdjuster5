using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
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

        private void Btn_SeletFile(object sender, RoutedEventArgs e)
        {
            Button inButton = (Button)sender;
            string strContent = inButton.Content.ToString();
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select File to Add To List (It will not auto start)",
                Filter = "All Files|*.*",
                InitialDirectory = System.IO.Path.GetFullPath(tbTargetDB.Text)
            };

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            if (result != null && result == true)
            {
                if (strContent[0].Equals('T'))
                    tbTargetDB.Text = dlg.FileName;
                else tbInputDB.Text = dlg.FileName;
            }
        }

        private void BtnSrcPresetGrps_Click(object sender, RoutedEventArgs e)
        {
            EditPresets myEP = new EditPresets(tbInputDB.Text,true,tbTargetDB.Text);
            myEP.Show();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // add checking saved changes here
        }

        private void BtnMovePresets_Click(object sender, RoutedEventArgs e)
        {
            MovePresets myMP = new MovePresets(tbTargetDB.Text, tbInputDB.Text);
            myMP.Show();
        }

        private void BtnTgtPresetGrps_Click(object sender, RoutedEventArgs e)
        {
            EditPresets myEP = new EditPresets(tbTargetDB.Text);
            myEP.Show();
        }
    }
}
