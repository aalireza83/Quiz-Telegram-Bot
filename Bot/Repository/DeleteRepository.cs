using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    class DeleteRepository : IDeleteRepository
    {
        string connectionString = constring.ConnectionString;

        public bool DeleteExists()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "SELECT COUNT(*) FROM TblDelete";
            OleDbCommand command = new OleDbCommand(query, connection);
            connection.Open();
            int userExists = (int)command.ExecuteScalar();
            connection.Close();
            if (userExists > 0)
                return true;
            else
                return false;
        }

        public bool Insert(long ExamCode, long ChatId, long MessageId)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Insert Into TblDelete (ExamCode,ChatId,MessageId) values (@ExamCode,@ChatId,@MessageId)";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@ExamCode", ExamCode);
                command.Parameters.AddWithValue("@ChatId", ChatId);
                command.Parameters.AddWithValue("@LastCommand", MessageId);
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

        public bool RemoveAll()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Delete From TblDelete";
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

        public DataTable SelectAll()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From TblDelete";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectDate()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select DISTINCT top 1 [Time] From DeleteReport ORDER BY [Time] ASC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectByDate(DateTime Date)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From DeleteReport Where Time=" + Date.ToOADate();
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public bool RemoveByDate(DateTime Date)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Select DISTINCT top 1 Code From DeleteReport Where Time=" + Date.ToOADate();
                OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                DataTable data = new DataTable();
                adapter.Fill(data);
                string query2 = "Delete From TblDelete Where ExamCode=" + int.Parse(data.Rows[0][0].ToString());
                OleDbCommand command = new OleDbCommand(query2, connection);
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
