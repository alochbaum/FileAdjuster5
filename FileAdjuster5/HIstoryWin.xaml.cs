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
        private DataTable m_DataTable = new DataTable();

        public HIstoryWin(bool IsActions=true)
        {
            bIsActions = IsActions;
            InitializeComponent();
            m_DateTime = DateTime.Now;
            dtpDate.Value = m_DateTime;
            LoadByDate(m_DateTime);
            //m_DataTable = FileAdjSQLite.GetHistRows(m_DateTime, bIsActions);
            // DGHistGrid.DataContext = m_DataTable.DefaultView;
            DGHistGrid.UpdateLayout();
        }

        //event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        //{
        //    //add
        //    //{
        //    //    throw new NotImplementedException();
        //    //}

        //    //remove
        //    //{
        //    //    throw new NotImplementedException();
        //    //}
        //}

        /// <summary>
        /// The date value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtpDate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DateTime dt = (DateTime)dtpDate.Value;
            //m_DataTable = FileAdjSQLite.GetHistRows(dt, bIsActions);
            LoadByDate(m_DateTime);
        }
        /// <summary>
        /// This loads the grid with all the Files or Actions
        /// </summary>
        /// <param name="inDT">The Date Value</param>
        /// <returns></returns>
        private Int64 LoadByDate(DateTime inDT)
        {
            Int64 iReturn = -1;
            m_DataTable = FileAdjSQLite.GetHistRows(inDT, bIsActions);
            return iReturn;
        }
        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            //if() add if for no selection maybe return false
            this.DialogResult = true;
        }
    }
}
