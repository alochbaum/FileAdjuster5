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
    /// Interaction logic for AddTime.xaml
    /// </summary>
    public partial class AddTime : Window
    {
        private bool bltpEndLoaded = false, bltsDurLoaded = false;
        public AddTime()
        {
            InitializeComponent();
        }

        public string GetParam1()
        {
            return tpStart.Text;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void TimeSpanUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(bltpEndLoaded&&bltsDurLoaded)
            tpEnd.Value = tpStart.Value + tsDur.Value;
        }

        private void tpEnd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (bltpEndLoaded && bltsDurLoaded)
                tsDur.Value = tpEnd.Value - tpStart.Value;
        }

        private void tsDur_Loaded(object sender, RoutedEventArgs e)
        {
            bltsDurLoaded = true;
        }

        private void tpEnd_Loaded(object sender, RoutedEventArgs e)
        {
            bltpEndLoaded = true;
        }
    }
}
