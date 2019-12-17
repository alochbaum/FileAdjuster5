using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace FileAdjuster5
{
    /// <summary>
    /// Interaction logic for HIstoryWin.xaml
    /// </summary>
    public partial class HIstoryWin : Window
    {
        private DateTime m_DateTime = new DateTime();
        private bool bIsActions = false;
        private DataTable m_DataTable;

        public HIstoryWin(bool IsActions=true)
        {
            bIsActions = IsActions;
            InitializeComponent();
            m_DateTime = DateTime.Now;
            // Adding one day to show today's results
            m_DateTime = m_DateTime.AddDays(1);
            dtpDate.Value = m_DateTime;
            //LoadByDate(m_DateTime);
        }

        /// <summary>
        /// The date value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtpDate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DateTime dt = (DateTime)dtpDate.Value;
            LoadByDate(dt);
        }
        /// <summary>
        /// This loads the grid with all the Files or Actions
        /// </summary>
        /// <param name="inDT">The Date Value</param>
        /// <returns></returns>
        private Int64 LoadByDate(DateTime inDT)
        {
            Int64 iReturn = -1;
            m_DataTable = new DataTable();
            m_DataTable = FileAdjSQLite.GetHistRows(inDT, bIsActions);
            DGchange.DataContext = m_DataTable.DefaultView;
            return iReturn;
        }
        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            //if() add if for no selection maybe return false
            this.DialogResult = true;
        }


    }
}
