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
    /// Interaction logic for GetString.xaml
    /// </summary>
    public partial class GetString : Window
    {
        private string strTitle = "";
        private string strQuestion = "";
        public GetString(string strTitleIn, string strQuestionIn)
        {
            strTitle = strTitleIn;
            strQuestion = strQuestionIn;
            InitializeComponent();
        }

        public string GetAnswer()
        {
            return tbAnswer.Text;
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            wndGetString.Title = strTitle;
            lbAnswer.Content = strQuestion;
        }
    }
}
