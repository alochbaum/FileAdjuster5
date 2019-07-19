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
        public List <CPreset> GetCPresets(string strDB)
        {
            List<CPreset> lreturn = new List<CPreset>();
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
                    lreturn.Add(new CPreset { iId = (Int64)reader["PTypeID"], Name = (string)reader["PresetType"] });
                    //lreturn.Add(new CPreset { iId = 0, Name = (string)reader["PresetType"] });
                }
                reader.Close();
                m_dbConnection.Close();
            }
            return lreturn;
        }
    }
}
