﻿using System;
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
    /// Interaction logic for OnAirSettings.xaml
    /// </summary>
    public partial class OnAirSettings : Window
    {
        private OnAirData myOnAirData = new OnAirData();
        private int iMaxData = -1,iCurrentPos=0;
        public OnAirSettings()
        {
            InitializeComponent();
            LoadOnAirData(0);
        }

        private void LoadOnAirData(int iDirection)
        {
            if (iCurrentPos != 0)
                myOnAirData = FileAdjSQLite.ReadOnAirData(iCurrentPos + iDirection);
            else
                myOnAirData = FileAdjSQLite.ReadOnAirData();
            if (!string.IsNullOrEmpty(myOnAirData.PreSetName))
            {
                tbPresetName.Text = myOnAirData.PreSetName;
                tbOutputFile.Text = myOnAirData.OutFileName;
                intLinesPerFile.Value = (int)myOnAirData.LongLinesPerFile;
                tbStartChar.Text = ((char)myOnAirData.IntStartChar).ToString();
                tbGrInChar.Text = ((char)myOnAirData.IntGroupChar).ToString();
                tbGrpOutChar.Text = ((char)myOnAirData.IntOutChar).ToString();
                intOffSet.Value = myOnAirData.IntGroupPos;
                if(iCurrentPos == 0) iMaxData = (int)myOnAirData.ID_OnAirData;
                iCurrentPos = (int) myOnAirData.ID_OnAirData;
                if (iCurrentPos > 1) btnPrevious.IsEnabled = true;
                else btnPrevious.IsEnabled = false;
                if (iCurrentPos < iMaxData) btnNext.IsEnabled = true;
                else btnNext.IsEnabled = false; 
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            myOnAirData.PreSetName = tbPresetName.Text;
            myOnAirData.OutFileName = tbOutputFile.Text;
            myOnAirData.LongLinesPerFile = (long)intLinesPerFile.Value;
            myOnAirData.IntStartChar = System.Convert.ToInt32(tbStartChar.Text[0]);
            myOnAirData.IntGroupChar = System.Convert.ToInt32(tbGrInChar.Text[0]);
            myOnAirData.IntOutChar = System.Convert.ToInt32(tbGrpOutChar.Text[0]);
            intOffSet.Value = myOnAirData.IntGroupPos;
            if(!FileAdjSQLite.WriteOnAir(myOnAirData))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(
                    "Couldn't save the changes made to OnAir Settings.",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.DialogResult = true;
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            LoadOnAirData(-1);
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            LoadOnAirData(1);
        }
    }
}
