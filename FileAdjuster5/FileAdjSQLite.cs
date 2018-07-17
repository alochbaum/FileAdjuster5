﻿using System;
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
        static public List<string> GetSizes()
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
        static public Int64 GetHistoryint()
        {
            Int64 iReturn = 0;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "Select group_id from FileHistory order by group_id desc limit 1;";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                        iReturn = reader.GetInt64(0);
                }
              
                reader.Close();
                m_dbConnection.Close();
            }
            return iReturn;
        }
        static public bool WriteHistory(Int64 iGroup,string strFileName)
        {
            bool blreturn = false;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sqlcmd = "INSERT INTO FileHistory (group_id,file_name" +
                   ")VALUES (" +iGroup.ToString()+",'"+
                    strFileName + "');";
                SQLiteCommand command = new SQLiteCommand(sqlcmd, m_dbConnection);
                int rows = command.ExecuteNonQuery();
                if (rows == 1) blreturn=true;
                m_dbConnection.Close();
            }
            return blreturn;
        }
        static public List<string> ReadHistory(Int64 lGroup)
        {
            List<string> mList = new List<string>();
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select * from FileHistory where group_id ="+
                    lGroup.ToString() +" order by id;";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    mList.Add(reader["file_Name"].ToString());
                reader.Close();
                m_dbConnection.Close();
            }
            return mList;
        }
    }
    
}
