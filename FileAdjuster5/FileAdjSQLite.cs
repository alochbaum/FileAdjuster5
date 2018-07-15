using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileAdjuster5
{
    static class FileAdjSQLite
    {
 
        static private string DBFile()
        {
            return AppDomain.CurrentDomain.BaseDirectory + "FileAdj.db";
        }
        static public List<string> getSizes()
        {
            List<string> mList = new List<string>();
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select * from SubFileSizes order by display_order;";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    mList.Add(reader["text"].ToString());
                reader.Close();
                m_dbConnection.Close();
            } else
            {
                mList.Add("5500000 Good Notepad++");
                mList.Add("550000 Great Notepad++");
                mList.Add("1048575 Excel Row Limit");
                mList.Add("55000 Dinky size");
            }
            return mList;
        }
    }
}
