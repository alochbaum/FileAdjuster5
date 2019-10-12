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
                string sql = "select ap.PTypeID, at.PresetType, ap.PresetName, ap.DateAdded" +
                     " from ActionPreset ap join ActionPresetType at on ap.PTypeID = at.PTypeID" +
                     " order by ap.PTypeID, ap.GroupID; ";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    lReturn.Add(new CDisplayPreset {
                        PresetID = (Int64)reader["PTypeID"],
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
