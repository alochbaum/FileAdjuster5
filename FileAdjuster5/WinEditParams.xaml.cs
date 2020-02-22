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
    /// Used to move action row data around
    /// </summary>
    public class ActionRowData
    {
        public string ActionType { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }

    }
    /// <summary>
    /// Interaction logic for WinEditParams.xaml
    /// </summary>
    public partial class WinEditParams : Window
    {
        public string strActionType = "";
        public WinEditParams(ActionRowData myParams,string strTitle)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(WinEditParams_Loaded);
            strActionType = myParams.ActionType;
            tbParam1.Text = myParams.Param1;
            tbParam2.Text = myParams.Param2;
            WinEdit.Title = strTitle;
        }
        private void RowType_Loaded(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = FileAdjSQLite.GetActionTypes();

            // ... Assign the ItemsSource to the List.
            RowType.ItemsSource = data;

            // ... Match with sent item
            RowType.SelectedValue = strActionType;
        }

        private void WinEditParams_Loaded(object sender, RoutedEventArgs e)
        {
            tbParam1.Focus();
        }

        public static ActionRowData GetEdit(ActionRowData inActionRow,string strTitle)
        {
            WinEditParams inst = new WinEditParams(inActionRow,strTitle);
            inst.ShowDialog();
            if (inst.DialogResult == true)
            {
                inActionRow.ActionType = inst.strActionType;
                inActionRow.Param1 = inst.tbParam1.Text;
                inActionRow.Param2 = inst.tbParam2.Text;
                return inActionRow;
            }
            else return null;
        }
        private void RowType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            strActionType = (sender as ComboBox).SelectedItem as string;
            if (strActionType == "Time_Window")
            {
                AddTime myTimeWnd = new AddTime();
                if (myTimeWnd.ShowDialog() == true)
                {
                    RowType.SelectedValue="Time_Window";
                    tbParam1.Text = myTimeWnd.GetParam1();
                    tbParam2.Text = myTimeWnd.GetParam2();
                }
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
