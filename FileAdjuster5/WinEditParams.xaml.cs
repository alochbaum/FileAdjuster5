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
    /// Used to move parameters around
    /// </summary>
    public class TwoParams
    {
        public string Param1 { get; set; }
        public string Param2 { get; set; }
    }
    /// <summary>
    /// Interaction logic for WinEditParams.xaml
    /// </summary>
    public partial class WinEditParams : Window
    {
        public WinEditParams(string strActionType, TwoParams myParams)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(WinEditParams_Loaded);
            lbType.Content = strActionType;
            tbParam1.Text = myParams.Param1;
            tbParam2.Text = myParams.Param2;
        }

        private void WinEditParams_Loaded(object sender, RoutedEventArgs e)
        {
            tbParam1.Focus();
        }

        public static TwoParams GetEdit(string ActionType, TwoParams in2Params)
        {
            WinEditParams inst = new WinEditParams(ActionType, in2Params);
            inst.ShowDialog();
            if (inst.DialogResult == true)
            {
                in2Params.Param1 = inst.tbParam1.Text;
                in2Params.Param2 = inst.tbParam2.Text;
                return in2Params;
            }
            else return null;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
