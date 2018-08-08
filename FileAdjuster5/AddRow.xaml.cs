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
    /// Interaction logic for AddRow.xaml
    /// </summary>
    public partial class AddRow : Window
    {
        public AddRow()
        {
            InitializeComponent();
        }

        private void RowType_Loaded(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = FileAdjSQLite.GetActionTypes();

            // ... Assign the ItemsSource to the List.
            RowType.ItemsSource = data;

            // ... Make the first item selected.
            RowType.SelectedIndex = 0;
        }

        public List<string> GetSettings()
        {
            List<string> LSreturn = new List<string>();
            LSreturn.Add(RowType.SelectedValue.ToString());
            LSreturn.Add(Param1.Text);
            LSreturn.Add(Param2.Text);
            return LSreturn;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void RowType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string strValue = (sender as ComboBox).SelectedItem as string;
            if (strValue == "Time_Window")
            {
                AddTime myTimeWnd = new AddTime();
                if (myTimeWnd.ShowDialog() == true)
                {
                    Param1.Text = myTimeWnd.GetParam1();
                    Param2.Text = myTimeWnd.GetParam2();
                }
            }
        }
    }
}
