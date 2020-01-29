using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
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
        private Int64 iOutGroup = -1;

        public Int64 GetOutGroup() { return iOutGroup; }

        public HIstoryWin(bool IsActions=true)
        {
            bIsActions = IsActions;
            InitializeComponent();
            m_DateTime = DateTime.Now;
            // Adding one day to show today's results
            m_DateTime = m_DateTime.AddDays(1);
            dtpDate.Value = m_DateTime;
            //LoadByDate(m_DateTime);
            if (IsActions) HistWin.Title = "Action Histories (Select one and OK to load)";
            else HistWin.Title = "File Histories (Select one and OK to load)";
        }

        /// <summary>
        /// The date value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtpDate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DateTime dt = (DateTime)dtpDate.Value;
            if (dt < (DateTime.Now.AddDays(-60)))
            {
                btnDelete.IsEnabled = true;
            } else { btnDelete.IsEnabled = false; }
            LoadByDate(dt);
        }
        /// <summary>
        /// This loads the grid with all the Files or Actions
        /// </summary>
        /// <param name="inDT">The Date Value</param>
        /// <returns></returns>
        private void LoadByDate(DateTime inDT)
        {
            m_DataTable = new DataTable();
            m_DataTable = FileAdjSQLite.GetHistRows(inDT, bIsActions);
            DGchange.DataContext = m_DataTable.DefaultView;
        }
        /// <summary>
        /// This is OK to load the selected event
        /// </summary>
        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            int iSelected = DGchange.SelectedIndex;
            if (iSelected >= 0)
            {
                DataRow row = m_DataTable.Rows[iSelected];
                DataRow newRow = m_DataTable.NewRow();
                newRow.ItemArray = row.ItemArray;
                iOutGroup = int.Parse(newRow["Group_ID"].ToString());
            }
            //if() add if for no selection maybe return false
            this.DialogResult = true;
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (m_DataTable.Rows.Count < 12) return;
            DataRow row = m_DataTable.Rows[12];
            DataRow newRow = m_DataTable.NewRow();
            newRow.ItemArray = row.ItemArray;
            string strDateAdded = newRow[1].ToString();
            try
            {
                DateTime dtDateTime = DateTime.ParseExact(strDateAdded, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                dtpDate.Value = dtDateTime;
            }
            catch (Exception)
            {

               // throw;
            }


            //m_DataTable = new DataTable();
            //m_DataTable = FileAdjSQLite.GetHistRows((Int64)iOrderNew, true, bIsActions);
            //DGchange.DataContext = m_DataTable.DefaultView;
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (m_DataTable.Rows.Count < 2) return;
            DataRow row = m_DataTable.Rows[0];
            DataRow newRow = m_DataTable.NewRow();
            newRow.ItemArray = row.ItemArray;
            string strGroup = newRow[0].ToString();
            m_DataTable = new DataTable();
            m_DataTable = FileAdjSQLite.GetNextDate(strGroup, bIsActions);
            DGchange.DataContext = m_DataTable.DefaultView;
            //MessageBox.Show($"Date Added {dtDateJump.ToShortDateString()}");
        }

    }
}
