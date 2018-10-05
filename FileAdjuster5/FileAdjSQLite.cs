using System;
using System.Data;
using System.Deployment.Application;
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
        private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static public string DBFile()
        {
            //return @"c:\iTX Software\FileAdj.db";
            // For the next line not to error you need to add System.Deployment reference
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                log.Debug("SQLite class is detecting that is Network Deployed");
                return ApplicationDeployment.CurrentDeployment.DataDirectory + @"\FileAdj.sqlite";
            }
            return @"C:\Users\andy\Source\Repos\FileAdjuster5\FileAdjuster5\FileAdj.sqlite";
        }
        static public List<string> GetSizes()
        {
            List<string> mList = new List<string>();
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            log.Debug($"Size function got strDBFile of {strDBFile}");
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
            }
            return mList;
        }
        static public List<string> GetActionTypes()
        {
            List<string> mList = new List<string>();
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            log.Debug($"Size function got strDBFile of {strDBFile}");
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select * from ActionType order by ActionTypeID;";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    mList.Add(reader["ActionType"].ToString());
                reader.Close();
                m_dbConnection.Close();
            }
            return mList;
        }
        static public List<string> GetPresetTypes()
        {
            List<string> mList = new List<string>();
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            log.Debug($"Size function got strDBFile of {strDBFile}");
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select * from ActionPresetType order by PTypeID;";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    mList.Add(reader["PresetType"].ToString());
                reader.Close();
                m_dbConnection.Close();
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
        static public Int64 GetActionint()
        {
            Int64 iReturn = 0;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "Select GroupID from ActionTable order by GroupID desc limit 1;";
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
        static public String GetActionDate(Int64 iGroup)
        {
            string strReturn = "";
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "Select DateAdded from ActionTable where GroupID = " 
                    + iGroup.ToString() + " order by TableID desc limit 1;";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                        strReturn = reader.GetString(0);
                }

                reader.Close();
                m_dbConnection.Close();
            }
            return strReturn;
        }

        static public Int64 GetPresetFlags(Int64 iGroup)
        {
            Int64 iReturn = 3;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select Flags from ActionPreset where GroupID = "
                    + iGroup.ToString() + ";";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                // This could return null
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
        static public bool WriteHistory(Int64 iGroup,string strFileName,string strExt)
        {
            bool blreturn = false;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sqlcmd = "INSERT INTO FileHistory (group_id,file_name,ext" +
                   ")VALUES (" +iGroup.ToString()+",@File,@Ext);";
                SQLiteCommand command = new SQLiteCommand(sqlcmd, m_dbConnection);
                // protected from single quotes in the passed strings
                command.Parameters.Add(new SQLiteParameter("File", strFileName));
                command.Parameters.Add(new SQLiteParameter("Ext", strExt));
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
                    mList.Add(reader["file_Name"].ToString()+"|"+reader["ext"].ToString()
                       + "|" + reader["date_added"].ToString());
                reader.Close();
                m_dbConnection.Close();
            }
            return mList;
        }
        //
        // Currently this function isn't protected against single quotes in the strings
        //
        static public bool WriteAction(Int64 iOrder, Int64 iGroup, string strType, string Param1, string Param2 )
        {
            bool blreturn = false;
            Int64 lType = 0;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select ActionTypeID from ActionType where ActionType='"+
                   strType +"';";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                        lType = reader.GetInt64(0);
                }
                reader.Close();
                if (lType > 0)
                {
                    sql = "INSERT INTO ActionTable (DisplayOrder,GroupID,ActionTypeID," +
                        "Parameter1,Parameter2) VALUES (" + iOrder.ToString() + "," +
                        iGroup.ToString() +"," + lType.ToString() + ",@Param1,@Param2);";
                    command = new SQLiteCommand(sql, m_dbConnection);
                    // protected from single quotes in the passed strings
                    command.Parameters.Add(new SQLiteParameter("Param1", Param1));
                    command.Parameters.Add(new SQLiteParameter("Param2", Param2));
                    int rows = command.ExecuteNonQuery();
                    if (rows == 1) blreturn = true;
                }
                m_dbConnection.Close();
            }
            return blreturn;
        }
        static public DataTable ReadActions(Int64 iGroup)
        {
            DataTable tableReturn = new DataTable();
            tableReturn.Columns.Add("Order", typeof(Int64));
            tableReturn.Columns.Add("Group", typeof(Int64));
            tableReturn.Columns.Add("Action", typeof(string));
            tableReturn.Columns.Add("Parameter1", typeof(string));
            tableReturn.Columns.Add("Parameter2", typeof(string));
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select at.DisplayOrder, at.GroupID, ty.ActionType," +
                    "at.Parameter1, at.Parameter2 from ActionTable at join ActionType ty " +
                    "on at.ActionTypeID = ty.ActionTypeID " +
                    "where at.GroupID="+ iGroup.ToString() +" order by at.DisplayOrder; ";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    tableReturn.Rows.Add(reader[0], reader[1], reader[2], reader[3], reader[4]);
                reader.Close();
                m_dbConnection.Close();
            }
            return tableReturn;
        }
        static public bool WriteGroup(string strGroup)
        {
            bool blreturn = false;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sqlcmd = "INSERT INTO ActionPresetType (PresetType) VALUES (@strGroup);";
                SQLiteCommand command = new SQLiteCommand(sqlcmd, m_dbConnection);
                // protected from single quotes in the passed strings
                command.Parameters.Add(new SQLiteParameter("strGroup", strGroup));
                int rows = command.ExecuteNonQuery();
                if (rows == 1) blreturn = true;
                m_dbConnection.Close();
            }
            return blreturn;
        }
        static public bool WritePreset(string strGroup, string strTitle, Int64 iGroup, Int64 iFlag)
        {
            bool blreturn = false;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sqlcmd = "insert into ActionPreset (PTypeId,PresetName,GroupID,Flags) " +
                    " select PTypeID,@strTitle,@strGroup,'" + iFlag.ToString() +
                    "' from ActionPresetType where PresetType = @strGroup;";
                SQLiteCommand command = new SQLiteCommand(sqlcmd, m_dbConnection);
                // protected from single quotes in the passed strings
                command.Parameters.Add(new SQLiteParameter("strGroup", strGroup));
                command.Parameters.Add(new SQLiteParameter("strTitle", strTitle));
                int rows = command.ExecuteNonQuery();
                if (rows == 1) blreturn = true;
                m_dbConnection.Close();
            }
            return blreturn;
        }
        static public DataTable ReadPresets()
        {
            DataTable tableReturn = new DataTable();
            tableReturn.Columns.Add("Group_ID", typeof(Int64));
            tableReturn.Columns.Add("Preset Group", typeof(string));
            tableReturn.Columns.Add("Preset Title", typeof(string));
            tableReturn.Columns.Add("Date Added", typeof(string));
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select ap.GroupID, at.PresetType, ap.PresetName, ap.DateAdded" +
                    " from ActionPreset ap join ActionPresetType at on ap.PTypeID = at.PTypeID"+
                    " order by ap.PTypeID, ap.GroupID; ";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    tableReturn.Rows.Add(reader[0], reader[1], reader[2], reader[3]);
                reader.Close();
                m_dbConnection.Close();
            }
            return tableReturn;
        }
    }
    
}
