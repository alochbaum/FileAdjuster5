using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private BackgroundWorker MyWorker2;
        public Window1()
        {
            InitializeComponent();
            MyWorker2 = (BackgroundWorker)this.FindResource("OnAirLog");
        }
        private void OnAirLog_DoWork(object sender, DoWorkEventArgs e) { }
        private void OnAirLog_ProgressChanged(object sender, ProgressChangedEventArgs e) { }
        private void OnAirLog_Complete(object sender, RunWorkerCompletedEventArgs e) { }
    }
}
