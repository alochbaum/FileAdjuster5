using System.Windows;

namespace FileAdj5DB
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
            tbAnswer.Focus();
        }
    }
}
