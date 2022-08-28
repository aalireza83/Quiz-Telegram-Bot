using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    class QuestionsRepository : IQuestionsRepository
    {
        string connectionString = constring.ConnectionString;
        public bool Delete(long ExamCode)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Delete From Questions Where ExamCode=" + ExamCode;
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

        public bool Insert(long ExamCode, int QuestionNumber, string Question, string Answer, int Key, int HardLevel)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Insert Into [Questions] (ExamCode,QuestionNumber,[Question],[Answer],[Key],HardLevel) values (@ExamCode,@QuestionNumber,@Question,@Answer,@Key,@HardLevel)";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@ExamCode", ExamCode);
                command.Parameters.AddWithValue("@QuestionNumber", QuestionNumber);
                command.Parameters.AddWithValue("@Question", Question);
                command.Parameters.AddWithValue("@Answer", Answer);
                command.Parameters.AddWithValue("@Key", Key);
                command.Parameters.AddWithValue("@HardLevel", HardLevel);
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

        public DataTable SelectByExamCode(long ExamCode)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From Questions Where ExamCode=" + ExamCode + " ORDER BY QuestionNumber ASC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }
    }
}
