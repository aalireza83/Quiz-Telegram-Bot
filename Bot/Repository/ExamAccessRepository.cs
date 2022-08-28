using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    class ExamAccessRepository : IExamAccessRepository
    {
        string connectionString = constring.ConnectionString;

        public bool Delete(long ChatId, long ExamCode)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Delete From ExamAccess where ChatId=" + ChatId + " AND ExamCode=" + ExamCode;
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

        public bool Exist(long ChatId, long ExamCode)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "SELECT COUNT(*) FROM AccessReport Where ChatId=" + ChatId + " AND Code=" + ExamCode;
            OleDbCommand command = new OleDbCommand(query, connection);
            connection.Open();
            int userExists = (int)command.ExecuteScalar();
            connection.Close();
            if (userExists > 0)
                return true;
            else
                return false;
        }

        public bool Insert(long ChatId, long ExamCode)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Insert Into ExamAccess (ChatId,ExamCode) values (@ChatId,@ExamCode)";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@ChatId", ChatId);
                command.Parameters.AddWithValue("@ExamCode", ExamCode);
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

        public DataTable SelectAll()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From AccessReport";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectByChatId(long ChatId)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From AccessReport Where ChatId=" + ChatId + " ORDER BY Time DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectByExam(long ExamCode)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From AccessReport Where Code=" + ExamCode + " ORDER BY Time DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }
        public DataTable SelectTop()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select top 10 * From AccessReport ORDER BY Time DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }
    }
}
