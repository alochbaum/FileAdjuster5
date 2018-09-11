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
    /// Interaction logic for SavePreset.xaml
    /// </summary>
    public partial class SavePreset : Window
    {
        private Int64 iGroupID = 0;
        private Int64 iFlags = 0;
        public SavePreset(Int64 inGroup, Int64 iFlagsInt)
        {
            iGroupID = inGroup;
            iFlags = iFlagsInt;
            InitializeComponent();
        }

        private void cbGroups_Loaded(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = FileAdjSQLite.GetPresetTypes();

            // ... Assign the ItemsSource to the List.
            cbGroups.ItemsSource = data;

            // ... Make the first item selected.
            cbGroups.SelectedIndex = 0;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            FileAdjSQLite.WritePreset(cbGroups.Text, tbTitle.Text, iGroupID, iFlags);
            this.DialogResult = true;
        }

        private void BtnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            GetString myGroup = new GetString("Enter Group Name","Preset Action Group Name?");
            if (myGroup.ShowDialog() == true)
            {
                FileAdjSQLite.WriteGroup(myGroup.GetAnswer());
                cbGroups_Loaded(null, new RoutedEventArgs());
            }

        }
    }
}
