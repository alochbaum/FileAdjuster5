using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileAdjuster5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Adding the version number to the title
            MainFrame.Title = "File Adjuster version:" + Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void cbLines_Loaded(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = new List<string>();
            data.Add("5500000 Good Notepad++");
            data.Add("550000 Great Notepad++");
            data.Add("1048575 Excel Row Limit");
            data.Add("55000 Dinky size");

            // ... Assign the ItemsSource to the List.
            cbLines.ItemsSource = data;

            // ... Make the first item selected.
            cbLines.SelectedIndex = 0;
        }
    }
}
