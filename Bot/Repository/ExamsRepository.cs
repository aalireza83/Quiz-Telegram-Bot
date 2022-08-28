using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    class ExamsRepository : IExamsRepository
    {
        string connectionString = constring.ConnectionString;
        public bool Delete(long Code)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Delete From Exams Where Code=" + Code;
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

        public bool ExamExist(long Code)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "SELECT COUNT(*) FROM Exams Where Code=" + Code;
            OleDbCommand command = new OleDbCommand(query, connection);
            connection.Open();
            int userExists = (int)command.ExecuteScalar();
            connection.Close();
            if (userExists > 0)
                return true;
            else
                return false;
        }

        public bool Insert(long Code, string Name, DateTime Date, DateTime Time)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Insert Into Exams values (@Code,@Name,@Date,@Time)";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@Code", Code);
                command.Parameters.AddWithValue("@Name", Name);
                command.Parameters.AddWithValue("@Date", Date.ToOADate());
                command.Parameters.AddWithValue("@Time", Time.ToOADate());
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

        public DataTable SelectRow(long Code)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From ExamsReport Where Code=" + Code;
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectAll()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From ExamsReport ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectTop()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select top 10 * From ExamsReport ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public bool UpdateTime(long Code, DateTime Time)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Update Exams Set [Time]=@Time Where Code=" + Code;
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@Time", Time);
                connection.Open();
                command.ExecuteNonQuery();
                return true;
            }
            catch(Exception ex)
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
