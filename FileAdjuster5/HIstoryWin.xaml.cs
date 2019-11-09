using System;
using System.Collections.Generic;
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
    /// Interaction logic for HIstoryWin.xaml
    /// </summary>
    public partial class HIstoryWin : Window
    {
        public HIstoryWin()
        {
            InitializeComponent();
            dtpDate.Value = DateTime.Now;
        }

        private void DtpDate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
