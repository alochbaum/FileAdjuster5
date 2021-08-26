using System;
using System.Data;
using System.Deployment.Application;
using System.Collections.Generic;
using System.Data.SQLite;
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
                return ApplicationDeployment.CurrentDeployment.DataDirectory + @"\FileAdj.sqlite";
            }
            // this would be debug mode, check for user files
            else
            {
                if(File.Exists(@"C:\Users\Andre\Source\Repos\alochbaum\FileAdjuster5\FileAdjuster5\FileAdj.sqlite")) 
                    return @"C:\Users\Andre\Source\Repos\alochbaum\FileAdjuster5\FileAdjuster5\FileAdj.sqlite";
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
            else log.Error($"Get sizes can't open db file {strDBFile}");
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
                string sql = "select * from ActionType order by ActionType;";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    mList.Add(reader["ActionType"].ToString());
                reader.Close();
                m_dbConnection.Close();
            }
            return mList;
        }
        static public bool ShiftPresetUp(string strValuePreset)
        {
            Int64 iCurrent = -1, iPreceding = -1;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
            m_dbConnection.Open();
            string sql = "select PTypeID from ActionPresetType where PresetType = '"+strValuePreset+"';";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                iCurrent = reader.GetInt64(0);
            reader.Close();
            if (iCurrent < 2)
            {
                m_dbConnection.Close();
                return false; // lowest number is 1, return if on lowest number
            }
            // Note: there might be some non-used numbers in the ActionPresetType table
            sql = "select distinct PTypeID from ActionPreset where PTypeID < "
                + iCurrent.ToString()+ " order by PTypeID desc limit 1";
            command = new SQLiteCommand(sql, m_dbConnection);
            reader = command.ExecuteReader();
            while (reader.Read())
                iPreceding = reader.GetInt64(0);
            reader.Close();
            if(iPreceding <1)
            {
                m_dbConnection.Close();
                return false;
            }
            // First change the Preset Type Table numbers
            sql = "UPDATE ActionPresetType SET PTypeID = 0 WHERE PTypeID = " + iCurrent.ToString();
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            sql = "UPDATE ActionPresetType SET PTypeID = " + iCurrent.ToString() + " WHERE PTypeID = " + iPreceding.ToString();
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            sql = "UPDATE ActionPresetType SET PTypeID = " + iPreceding.ToString() + " WHERE PTypeID = 0;";
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            // Second change the action presets
            sql = "UPDATE ActionPreset SET PTypeID = 0 WHERE PTypeID = " + iCurrent.ToString();
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            sql = "UPDATE ActionPreset SET PTypeID = "+iCurrent.ToString()+" WHERE PTypeID = " + iPreceding.ToString();
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            sql = "UPDATE ActionPreset SET PTypeID = " + iPreceding.ToString() + " WHERE PTypeID = 0;";
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
            return true;
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
                string sql = "select * from ActionPresetType order by PresetType;";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    mList.Add(reader["PresetType"].ToString());
                reader.Close();
                m_dbConnection.Close();
            }
            else log.Error($"Get preset tupes can't open db file {strDBFile}");
            return mList;
        }
        static public Int64 GetFileHistoryInt()
        {
            Int64 iReturn = 0;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            m_dbConnection.ConnectionString = "Data Source=" + DBFile() + ";Version=3;";
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
            return iReturn;
        }
        static public Int64 GetActionint()
        {
            Int64 iReturn = 0;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            m_dbConnection.ConnectionString = "Data Source=" + DBFile() + ";Version=3;";
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
            return iReturn;
        }
        static public Int64 GetActionIDByName(string strNamePreset)
        {
            Int64 iReturn = 0;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            m_dbConnection.ConnectionString = "Data Source=" + DBFile() + ";Version=3;";
            m_dbConnection.Open();
            string sql = $"Select GroupID from ActionPreset where PresetName ='{strNamePreset}' limit 1;";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                    iReturn = reader.GetInt64(0);
            }
            reader.Close();
            m_dbConnection.Close();
            return iReturn;

        }
        static public String GetActionDate(Int64 iGroup)
        {
            string strReturn = "";
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            m_dbConnection.ConnectionString = "Data Source=" + DBFile() + ";Version=3;";
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
            return strReturn;
        }

        static public Int64 GetPresetFlags(Int64 iGroup)
        {
            Int64 iReturn = 3,iRowsAfter=0;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select Flags,RowsAfter from ActionPreset where GroupID = "
                    + iGroup.ToString() + ";";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                // This could return null
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (reader[0].GetType() != typeof(DBNull))
                        {
                            iReturn = reader.GetInt64(0);
                        }
                        if(reader[1].GetType() != typeof(DBNull))
                        {
                            iRowsAfter = reader.GetInt64(1);
                        }

                    }
                }

                reader.Close();
                m_dbConnection.Close();
            }
            //if (iRowsAfter > 0)
            iReturn += (iRowsAfter << 7);
            return iReturn;
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
        /// <summary>
        /// The function writes action to database, using parameters in case there are single quotes
        /// in the parameter strings
        /// </summary>
        /// <param name="iOrder">display order on screen</param>
        /// <param name="iGroup">group of the action</param>
        /// <param name="strType">action type number not really a string</param>
        /// <param name="Param1"></param>
        /// <param name="Param2"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Reads one line in to OnAirData object
        /// </summary>
        /// <param name="useID">If used selects OnAir_ID row</param>
        /// <returns></returns>
        static public OnAirData ReadOnAirData(Int64 useID=0)
        {
            OnAirData dataReturn = new OnAirData();
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select * from OnAir order by OnAir_ID desc limit 1 ";
                if (useID > 0) sql = $"select * from OnAir where OnAir_ID={useID}";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dataReturn.PreSetName = (string)reader[1];
                    dataReturn.OutFileName = (string)reader[2];
                    dataReturn.IntStartChar = (int)reader.GetInt64(3);
                    dataReturn.IntGroupChar = (int)reader.GetInt64(4);
                    dataReturn.IntGroupPos = (int)reader.GetInt64(5);
                    dataReturn.IntOutChar = (int)reader.GetInt64(6);
                    dataReturn.LongLinesPerFile = (long)reader.GetInt64(7);
                }
                reader.Close();
                m_dbConnection.Close();
            }
            return dataReturn;
        }
        static public Int64 CountOfOnAir()
        {
            Int64 lgReturn=1;
            return lgReturn;
        }
        static public String ReadVersion(string strDBFile)
        {
            string strReturn = "";
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select name from sqlite_master where type='table' and name ='Version'";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    strReturn = reader.GetString(0);
                reader.Close();
                if (strReturn.Length > 1)
                {
                    sql = "select Number from Version order by Version_ID desc limit 1";
                    command = new SQLiteCommand(sql, m_dbConnection);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                        strReturn = reader.GetString(0);
                    reader.Close();
                }
                m_dbConnection.Close();
            }
            return strReturn;
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
        static public bool WriteFileHistory(Int64 iGroup, string strFileName, string strExt)
        {
            bool blreturn = false;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sqlcmd = "INSERT INTO FileHistory (group_id,file_name,ext" +
                   ")VALUES (" + iGroup.ToString() + ",@File,@Ext);";
                SQLiteCommand command = new SQLiteCommand(sqlcmd, m_dbConnection);
                // protected from single quotes in the passed strings
                command.Parameters.Add(new SQLiteParameter("File", strFileName));
                command.Parameters.Add(new SQLiteParameter("Ext", strExt));
                int rows = command.ExecuteNonQuery();
                if (rows == 1) blreturn = true;
                m_dbConnection.Close();
            }
            return blreturn;
        }
        /// <summary>
        /// This saves preset to database which points to a group of already saved action rows
        /// However the the flags are not saved rows, and if group is negative it is only flags
        /// </summary>
        /// <param name="strGroup"></param>
        /// <param name="strTitle"></param>
        /// <param name="iGroup"></param>
        /// <param name="iFlag"> This also include 0 to 50 in the high bits</param>
        /// <returns></returns>
        static public bool WritePreset(string strGroup, string strTitle, Int64 iGroup, Int64 iFlag, int iRowsAfter)
        {
            bool blreturn = false;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sqlcmd = "insert into ActionPreset (PTypeId,PresetName,GroupID,Flags,RowsAfter) " +
                    " select PTypeID,@strTitle,'"+ iGroup.ToString() +"','" + iFlag.ToString() +
                    "','" + iRowsAfter.ToString()+"'"+
                    " from ActionPresetType where PresetType = @strGroup;";
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
        static public Int64 GetPresetGroup(string strGroup)
        {
            Int64 i64Return = -1;
            string strDBFile = DBFile();
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select PTypeID from ActionPresetType where PresetType = '" + strGroup + "';";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    i64Return= reader.GetInt64(0);
                m_dbConnection.Close();
            }
            return i64Return;
        }
        static public DataTable ReadPresets(string strDBFile="")
        {
            DataTable tableReturn = new DataTable();
            tableReturn.Columns.Add("Group", typeof(Int64));
            tableReturn.Columns.Add("Preset Group", typeof(string));
            tableReturn.Columns.Add("Preset Title", typeof(string));
            tableReturn.Columns.Add("Date Added", typeof(string));
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            // check if this is outside DB file name, if it isn't use the default name
            if(strDBFile.Length<3)strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select ap.GroupID, at.PresetType, ap.PresetName, ap.DateAdded" +
                    " from ActionPreset ap join ActionPresetType at on ap.PTypeID = at.PTypeID" +
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
        /// <summary>
        /// This is the overloaded Function to return 13 history rows based on group number
        /// </summary>
        /// <param name="iInGroupID">Group number to start at</param>
        /// <param name="IsPrevious">True seach eq or less; False eq or greater</param>
        /// <param name="IsActionRows">True is Action Rows; False is Files List</param>
        /// <returns></returns>
        static public DataTable GetHistRows(Int64 iInGroupID, bool IsPrevious, bool IsActionRows)
        {
            DataTable tableReturn = new DataTable();
            tableReturn.Columns.Add("Group ID", typeof(Int64));
            tableReturn.Columns.Add("Date Added", typeof(string));
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
            if (IsActionRows)
            {
                tableReturn.Columns.Add("Parameter1", typeof(string));
                tableReturn.Columns.Add("Parameter2", typeof(string));
                string sql;
                if (File.Exists(strDBFile))
                {
                    m_dbConnection.Open();
                    // Don't Need Previous section because next sends number
                    //if (IsPrevious)
                    //    sql = "select GroupID,DateAdded,Parameter1,Parameter2 from ActionTable where DisplayOrder=1" +
                    //        " and GroupID <= " + iInGroupID.ToString() + " order by DateAdded desc limit 13; ";
                    //else
                        sql = "select GroupID,DateAdded,Parameter1,Parameter2 from ActionTable where DisplayOrder=1" +
                            " and GroupID <= " + iInGroupID.ToString() + " order by DateAdded desc limit 13; ";
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                        tableReturn.Rows.Add(reader[0], reader[1], reader[2], reader[3]);
                    reader.Close();
                    m_dbConnection.Close();
                }
            }
            else // Doing Files History
            {
                tableReturn.Columns.Add("FileName", typeof(string));
                if (File.Exists(strDBFile))
                {
                    m_dbConnection.Open();
                    string sql;
                    // Don't need Previous Section because next sends group number
                    //if (IsPrevious)
                    //    sql = "select group_id,date_added,file_name from FileHistory" +
                    //      " where group_id <= " + iInGroupID.ToString() + " order by date_added desc limit 13; ";
                    //else
                        sql = "select group_id,date_added,file_name from FileHistory" +
                        " where group_id <= " + iInGroupID.ToString() + " group by group_id order by date_added desc limit 13; ";
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                        tableReturn.Rows.Add(reader[0], reader[1], reader[2]);
                    reader.Close();
                    m_dbConnection.Close();
                }
            }
            return tableReturn;        
        }
        /// <summary>
        /// From group number it seeks table with next group
        /// </summary>
        /// <param name="inDateTime"></param>
        /// <param name="iLimit"></param>
        /// <returns></returns>
        static public DataTable GetNextDate(string strGroup,bool IsActionRows)
        {
            DataTable tableReturn = new DataTable();
            Int64 iGroup=0;
            tableReturn.Columns.Add("Group ID", typeof(Int64));
            tableReturn.Columns.Add("Date Added", typeof(string));
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            m_dbConnection.ConnectionString = "Data Source=" + DBFile() + ";Version=3;";
            m_dbConnection.Open();
            //string strDateTime = inDateTime.ToString("yyyy-MM-dd 00:00:00");
            string sql = "";
            if (IsActionRows)
            {
                sql = "select max(GroupID) from (Select GroupID from ActionTable where GroupID>=" + strGroup + " group by GroupID limit 13); ";
            } else
            {
                sql = "select max(group_id) from (Select group_id from FileHistory where group_id>=" + strGroup + " group by group_id limit 13); ";
            }
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                iGroup = reader.GetInt64(0);
            }
            reader.Close();
            m_dbConnection.Close();
            if (iGroup>0)
            {
                tableReturn = GetHistRows(iGroup, false, IsActionRows);
            }
            return tableReturn;
        }
        /// <summary>
        /// This is the overloaded Function to return 13 history rows based on date
        /// </summary>
        /// <param name="inDateTime">Date Time to search</param>
        /// <param name="IsActionRows">True is Action Rows; False is Files List</param>
        /// <returns></returns>
        static public DataTable GetHistRows(DateTime inDateTime, bool IsActionRows)
        {
            DataTable tableReturn = new DataTable();
            tableReturn.Columns.Add("Group ID", typeof(Int64));
            tableReturn.Columns.Add("Date Added", typeof(string));
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            string strDateTime = inDateTime.ToString("yyyy-MM-dd 00:00:00");
            m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
            if (IsActionRows)
            {
                tableReturn.Columns.Add("Parameter1", typeof(string));
                tableReturn.Columns.Add("Parameter2", typeof(string));
                if (File.Exists(strDBFile))
                {
                    m_dbConnection.Open();
                    // Select GroupID,DateAdded,Parameter1,Parameter2 from ActionTable where DisplayOrder=1 and DateAdded <= datetime('2019-10-01 00:00:00');
                    string sql = "select GroupID,DateAdded,Parameter1,Parameter2 from ActionTable where DisplayOrder=1" +
                        " and DateAdded <= datetime('"+strDateTime+ "') order by DateAdded desc limit 13; ";
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                        tableReturn.Rows.Add(reader[0], reader[1], reader[2], reader[3]);
                    reader.Close();
                    m_dbConnection.Close();
                }
            }
            else
            {
                tableReturn.Columns.Add("FileName", typeof(string));
                if (File.Exists(strDBFile))
                {
                    m_dbConnection.Open();
                    // select group_id,date_added,file_name from FileHistory order by group_id limit 1
                    string sql = "select group_id,date_added,file_name from FileHistory" +
                        " where date_added <= datetime('" + strDateTime + "') group by group_id order by date_added desc limit 13; ";
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                        tableReturn.Rows.Add(reader[0], reader[1], reader[2]);
                    reader.Close();
                    m_dbConnection.Close();
                }
            }
            return tableReturn;
        }
        /// <summary>
        /// This is function to export presets to a new database
        /// </summary>
        /// <param name="StrFile">The file name to use for saved database</param>
        static public void ExportPresets(string StrFile)
        {
            SQLiteConnection.CreateFile(StrFile);
            string strDBFile = DBFile();
            if (File.Exists(strDBFile))
            {
                SQLiteConnection m_dbConnection = new SQLiteConnection();
                m_dbConnection.ConnectionString = "Data Source=" +StrFile + ";Version=3;";
                m_dbConnection.Open();
                string sql = "CREATE TABLE 'ActionPreset' ( 'PresetID' INTEGER PRIMARY KEY AUTOINCREMENT, 'PTypeID' INTEGER, 'PresetName' TEXT, 'GroupID' INTEGER," +
                    " 'Flags' INTEGER, 'RowsAfter' INTEGER, 'DateAdded' TEXT );";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                sql = "CREATE TABLE 'ActionPresetType' ( 'PTypeID' INTEGER PRIMARY KEY AUTOINCREMENT, 'PresetType' TEXT );";
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                sql = "CREATE TABLE 'Version' ( 'Version_id' INTEGER PRIMARY KEY AUTOINCREMENT, 'Number' TEXT, 'Title' TEXT, 'Notes' TEXT );";
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                sql = "CREATE TABLE 'ActionTable' ( 'TableID' INTEGER PRIMARY KEY AUTOINCREMENT, 'DisplayOrder' INTEGER, 'GroupID' INTEGER, 'ActionTypeID' INTEGER, 'Parameter1' TEXT, 'Parameter2' TEXT, 'DateAdded' TEXT );";
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                sql = "ATTACH '"+strDBFile+"' AS md;";
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                sql = "insert into Version select * from md.Version;";
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                sql = "insert into ActionPreset select * from md.ActionPreset;";
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                sql = "insert into ActionPresetType select * from md.ActionPresetType;";
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                sql = "insert into ActionTable select at.* from md.ActionTable at join md.ActionPreset ap where at.GroupID = ap.GroupID;";
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                m_dbConnection.Close();

            }
        }
        static public string ImportAssets(string strFile, List<CDisplayPreset> my_ListPresets,string strVersion,Int64 iActGrp)
        {
            string strReturn = "",strSQLwrite="",strDate="";
            string strDBFile = DBFile();
            Int64 iRowsAfter = 0, iFlags=0;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
            m_dbConnection.Open();
            string sql = "ATTACH '" + strFile + "' AS md;";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            // need to set this up as parameter for final version
            if (strVersion == "5.10")
            {
                // we have PTypeID, and GroupID should be gotten by next available group
                sql = "select Flags,DateAdded,RowsAfter from md.ActionPreset where PresetName = @PresetName";

            }
            else
            {
                sql = "select Flags,DateAdded from md.ActionPreset where PresetName = @PresetName";

            }
            strSQLwrite = "INSERT INTO ActionPreset (PTypeID,PresetName,GroupID,Flags,RowsAfter,DateAdded) VALUES (" +
                "@PTypeID,@PresetName,@GroupID,@Flags,@RowsAfter,@DateAdded)";
            foreach (CDisplayPreset DP in my_ListPresets)
            {
                command = new SQLiteCommand(sql, m_dbConnection);
                command.Parameters.Add(new SQLiteParameter("PresetName", DP.PresetName));
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    iFlags = reader.GetInt64(0);
                    strDate = reader.GetString(1);
                    if (strVersion == "5.10") iRowsAfter = reader.GetInt64(2);

                }
                reader.Close();
                command = new SQLiteCommand(strSQLwrite, m_dbConnection);
                // protected from single quotes in the passed strings
                command.Parameters.Add(new SQLiteParameter("PTypeID", DP.PresetTypeID));
                command.Parameters.Add(new SQLiteParameter("PresetName", DP.PresetName));
                if (DP.GroupID != -1)  // This special modifiers group
                {
                    iActGrp++;
                    command.Parameters.Add(new SQLiteParameter("GroupID", iActGrp));
                } else command.Parameters.Add(new SQLiteParameter("GroupID", -1));
                command.Parameters.Add(new SQLiteParameter("Flags", iFlags));
                command.Parameters.Add(new SQLiteParameter("RowsAfter", iRowsAfter));
                command.Parameters.Add(new SQLiteParameter("DateAdded", strDate));
                command.ExecuteNonQuery();
                string SQLTemp = "DROP TABLE IF EXISTS Actions_1";
                command = new SQLiteCommand(SQLTemp, m_dbConnection);
                command.ExecuteNonQuery();
                SQLTemp = "CREATE TEMP TABLE Actions_1 ( 'TableID' INTEGER PRIMARY KEY AUTOINCREMENT, 'DisplayOrder' INTEGER, 'GroupID' INTEGER, 'ActionTypeID' INTEGER, 'Parameter1' TEXT, 'Parameter2' TEXT, 'DateAdded' TEXT )";
                command = new SQLiteCommand(SQLTemp, m_dbConnection);
                command.ExecuteNonQuery();
                SQLTemp = "insert into Actions_1 select * from md.ActionTable where GroupID = " +
                    DP.GroupID.ToString();
                command = new SQLiteCommand(SQLTemp, m_dbConnection);
                command.ExecuteNonQuery();
                SQLTemp = "update Actions_1 set GroupID = "+iActGrp.ToString()+" where GroupID = " +
                    DP.GroupID.ToString();
                command = new SQLiteCommand(SQLTemp, m_dbConnection);
                command.ExecuteNonQuery();
                SQLTemp = "insert into ActionTable (DisplayOrder,GroupID,ActionTypeID,Parameter1,Parameter2) "+
                    "select DisplayOrder,GroupID,ActionTypeID,Parameter1,Parameter2 from Actions_1";
                command = new SQLiteCommand(SQLTemp, m_dbConnection);
                command.ExecuteNonQuery();
            }
            m_dbConnection.Close();
            return strReturn;
        }
        static public string DeletePreset(List<string> lsIn)
        {
            string strReturn = "",sql="";
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            string strDBFile = DBFile();
            m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
            if (File.Exists(strDBFile))
            {
                m_dbConnection.Open();
                foreach (string strPreset in lsIn)
                {
                    sql = "delete from ActionPreset where PresetName = '" + strPreset + "';";
                    SQLiteCommand lcommand = new SQLiteCommand(sql, m_dbConnection);
                    int rows = lcommand.ExecuteNonQuery();
                    if (rows < 1) strReturn += $"Error deleting: {strPreset}  ";
                }
                // now clean up presetypes that are empty
                sql = "delete from ActionPresetType where PTypeID in (select at.PTypeID from ActionPresetType at left join ActionPreset ap on at.PTypeID = ap.PTypeID where ap.PTypeID is null);";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                m_dbConnection.Close();
            }
            return strReturn;
        }
        static public string DeletePriorGroup(string StrGroup,bool blIsActions)
        {
            string strReturn = "";
                SQLiteConnection m_dbConnection = new SQLiteConnection();
                string strDBFile = DBFile();
                if (File.Exists(strDBFile))
                {
                m_dbConnection.ConnectionString = "Data Source=" + strDBFile + ";Version=3;";
                m_dbConnection.Open();
                string sqlcmd = "";
                if (blIsActions)
                {
                    sqlcmd = "delete from ActionTable where GroupID in (select act.GroupID"+
                        " From ActionTable as act left outer join ActionPreset as ap on"+
                        " act.GroupID = ap.GroupID where ap.GroupID is null and act.GroupID < @strGroup)";
                } else {
                    sqlcmd = $"DELETE FROM FileHistory WHERE group_id < @strGroup;";
                }
                SQLiteCommand command = new SQLiteCommand(sqlcmd, m_dbConnection);
                    // protected from single quotes in the passed strings
                    command.Parameters.Add(new SQLiteParameter("strGroup", StrGroup));
                    int rows = command.ExecuteNonQuery();
                    if (rows > 1) strReturn = StrGroup;
                    m_dbConnection.Close();
            }
            return strReturn;
        }
    }
    
}
