using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace FileAdj5DB
{
    class SqLiteModifer
    {
        public List<CPresetType> GetCPresetsTypes(string strDB)
        {
            List<CPresetType> lreturn = new List<CPresetType>();
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            if (File.Exists(strDB))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDB + ";Version=3;";
                m_dbConnection.Open();

                string sql = "select * from ActionPresetType order by PTypeID;";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    lreturn.Add(new CPresetType { iId = (Int64)reader["PTypeID"], Name = (string)reader["PresetType"] });
                    //lreturn.Add(new CPresetType { iId = 0, Name = (string)reader["PresetType"] });
                }
                reader.Close();
                m_dbConnection.Close();
            }
            return lreturn;
        }

        public List<CDisplayPreset> GetDisplayPresets(string strDB)
        {
            List<CDisplayPreset> lReturn = new List<CDisplayPreset>();
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            if (File.Exists(strDB))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDB + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select ap.PresetID, at.PresetType, ap.PresetName, ap.DateAdded" +
                     " from ActionPreset ap join ActionPresetType at on ap.PTypeID = at.PTypeID" +
                     " order by ap.PTypeID, ap.GroupID; ";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    lReturn.Add(new CDisplayPreset {
                        PresetID = (Int64)reader["PresetID"],
                        PresetTypeName = (string)reader["PresetType"],
                        PresetName = (string)reader["PresetName"],
                        Date = (string)reader["DateAdded"]
                    });
                }
                reader.Close();
                m_dbConnection.Close();
            }
            return lReturn;
        }
        /// <summary>
        /// This just checks to see if PresetType Name is in the incoming database
        /// </summary>
        /// <param name="strDB"></param>
        /// <param name="strPresetType"></param>
        /// <returns></returns>
        public bool GetIsPresetType(string strDB,string strPresetType)
        {
            bool blReturn = false;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            if (File.Exists(strDB))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDB + ";Version=3;";
                m_dbConnection.Open();
                string sql = "select PresetType from ActionPresetType where PresetType = '" +
                    strPresetType+"';";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    blReturn = true;
                }
                m_dbConnection.Close();
            }
            return blReturn;
        }
        /// <summary>
        /// This adds a row to ActionPresetType
        /// </summary>
        /// <param name="strName"></param>
        /// This is name of PresetType to add
        /// <param name="strTargetDB"></param>
        /// This is file name of the SQLite3 database 
        /// <returns></returns>
        /// Returns an "Done" string for good results or error message
        public string AddPresetType(string strName,string strTargetDB)
        {
   
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            if (File.Exists(strTargetDB))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strTargetDB + ";Version=3;";
                m_dbConnection.Open();
                string sql = "INSERT INTO ActionPresetType (PresetType) VALUES (@data);";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("data", strName);
                try
                {
                    command.ExecuteNonQuery();
                    m_dbConnection.Close();
                }
                catch (Exception Ex)
                {
                    return Ex.Message;
                }
            }
            return "Done";
        }
        public string MovePreset(string strSourceDB, string strTargetDB, string strPresetName)
        {
            Int64 iGroupID = GetHighestGroupID(strTargetDB);
            if (iGroupID < 1) return "Couldn't get groupID in Target DB";
            iGroupID++;
            CPreset myPreset = GetPreset(strSourceDB, strPresetName);
            if (myPreset == null) return "Problem reading Preset in Source DB";
            List<CAction> ListAction = GetActions(strSourceDB, myPreset.GroupID.ToString());
            return WritePreset(strTargetDB, myPreset, iGroupID);
            //return $"Done";
        }
 //       static public bool WritePreset(string strGroup, string strTitle, Int64 iGroup, Int64 iFlag)
        static public string WritePreset(string strTargetDB,CPreset myPreset,Int64 GroupID)
        {
            string rStr = "Error";
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            if (File.Exists(strTargetDB))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strTargetDB + ";Version=3;";
                m_dbConnection.Open();
                string sqlcmd = "Insert Into ActionPreset (PTypeId,PresetName,GroupID,Flags) Values (" +
                     myPreset.PTypeID.ToString() + ",'" + myPreset.PresetName.ToString() + "'," + 
                     GroupID.ToString() + ","+ myPreset.Flags.ToString() +");";
                SQLiteCommand command = new SQLiteCommand(sqlcmd, m_dbConnection);
                // protected from single quotes in the passed strings
                //command.Parameters.Add(new SQLiteParameter("strGroup", strGroup));
                //command.Parameters.Add(new SQLiteParameter("strTitle", strTitle));
                try
                {
                    command.ExecuteNonQuery();
                    m_dbConnection.Close();
                }
                catch (Exception Ex)
                {
                    return Ex.Message;
                }
            }
            return rStr;
        }
        public List<CAction> GetActions(string strSourceDB,string strGroupID)
        {
            List<CAction> rListAction = new List<CAction>();
            CAction myAction = new CAction();
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            if (File.Exists(strSourceDB))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strSourceDB + ";Version=3;";
                m_dbConnection.Open();
                string sql = "Select DisplayOrder,GroupID,ActionTypeID,Parameter1,Parameter2,DateAdded From ActionTable Where GroupID = @data; ";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("data", strGroupID);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        myAction.DisplayOrder = reader.GetInt64(0);
                        myAction.GroupID = reader.GetInt64(1);
                        myAction.ActionTypeID = reader.GetInt64(2);
                        myAction.Parameter1 = reader.GetString(3);
                        myAction.Parameter2 = reader.GetString(4);
                        myAction.DateAdded = reader.GetString(5);
                        rListAction.Add(myAction);
                    }
                }
                reader.Close();
                m_dbConnection.Close();
                // after closing there could be no HasRows, so sending null if Name not set
                return rListAction;
            }
            return rListAction;
        }
        public CPreset GetPreset(string strSourceDB,string strPresetName)
        {
            CPreset rPreset = new CPreset();
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            if (File.Exists(strSourceDB))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strSourceDB + ";Version=3;";
                m_dbConnection.Open();
                string sql = "Select PTypeID,GroupID,Flags,DateAdded From ActionPreset Where PresetName=@data;";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("data", strPresetName);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        rPreset.PTypeID = reader.GetInt64(0);
                        rPreset.GroupID = reader.GetInt64(1);
                        rPreset.PresetName = strPresetName;
                        rPreset.Flags = reader.GetInt64(2);
                        rPreset.DateAdded = reader.GetString(3);
                    }
                }
                reader.Close();
                m_dbConnection.Close();
                // after closing there could be no HasRows, so sending null if Name not set
                return rPreset;
            }
            return null;
        }
        public Int64 GetHighestGroupID(string strDB)
        {
            Int64 iReturn = 0;
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            if (File.Exists(strDB))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDB + ";Version=3;";
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
        public bool RenamePresetType(string strDB, string strId, string strNew)
        {
            SQLiteConnection m_dbConnection = new SQLiteConnection();
            if (File.Exists(strDB))
            {
                m_dbConnection.ConnectionString = "Data Source=" + strDB + ";Version=3;";
                m_dbConnection.Open();
                string sql = "UPDATE ActionPresetType SET PresetType='" + strNew + "' WHERE PTypeID=" + strId + ";";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                // this command is getting error datebase is locked when database is open in other program
                command.ExecuteNonQuery();
                m_dbConnection.Close();
                return true;
            }
            return false;
        }
    }
}
