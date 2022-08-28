using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    class SettingsRepository : ISettingsRepository
    {
        string connectionString = Repository.constring.ConnectionString;
        public DataTable SelectSettings()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From BotSettings";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public bool UpdateSettings(bool StopBot, bool Puesd, bool QuestionByQuestion, bool AnswerSheet, string FileId, bool AutoBackUp)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "UPDATE BotSettings SET StopBot=" + StopBot + " , Puesd=" + Puesd + ", QuestionByQuestion=" + QuestionByQuestion + " , AnswerSheet=" + AnswerSheet + " , FileId='" + FileId + "' , AutoBackUp=" + AutoBackUp + " WHERE BotName='" + "Quiz" + "'";
                OleDbCommand command = new OleDbCommand(query, connection);
                connection.Open();
                command.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
