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
        private int iOtherChanges = 3;
        //private bool bltpEndLoaded = false, bltsDurLoaded = false;
        public AddTime()
        {
            InitializeComponent();
        }

        public string GetParam1()
        {
            return tpStart.Text;
        }

        public string GetParam2()
        {
            return tpEnd.Text;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }


        private void myValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (iOtherChanges < 1)
            {
                if (sender.Equals(tpEnd))
                {
                    tsDur.Value = tpEnd.Value - tpStart.Value;
                }
                else if (sender.Equals(tsDur) || (sender.Equals(tpStart)))
                {
                    tpEnd.Value = tpStart.Value + tsDur.Value;
                }
            }
            else iOtherChanges--;
        }

        private void tpEnd_Loaded(object sender, RoutedEventArgs e)
        {
            iOtherChanges = 0;
        }
    }
}
