using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    class PayRepository : IPayRepository
    {
        string connectionString = constring.ConnectionString;
        public bool Delete(long TrackingCode)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Delete From Pay where TrackingCode=" + TrackingCode;
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

        public bool Insert(long TrackingCode, long ChatId, long Amount, string UserName, string Name, string Phone, string CardNumber, DateTime Date, string Description)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Insert Into Pay values (@TrackingCode,@ChatId,@Amount,@UserName,@Name,@Phone,@CardNumber,@Date,@Description)";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@TrackingCode", TrackingCode);
                command.Parameters.AddWithValue("@ChatId", ChatId);
                command.Parameters.AddWithValue("@Amount", Amount);
                command.Parameters.AddWithValue("@UserName", UserName);
                command.Parameters.AddWithValue("@Name", Name);
                command.Parameters.AddWithValue("@Phone", Phone);
                command.Parameters.AddWithValue("@CardNumber", CardNumber);
                command.Parameters.AddWithValue("@Date", Date.ToOADate());
                command.Parameters.AddWithValue("@Description", Description);
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
            string query = "Select * From Pay ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable Selectrow(long TrackingCode)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From Pay Where TrackingCode=" + TrackingCode + " ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectTop()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "SELECT top 10 * FROM Pay ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public bool PayExist(long TrackingCode)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "SELECT COUNT(*) FROM Pay WHERE TrackingCode=" + TrackingCode;
            connection.Open();
            OleDbCommand command = new OleDbCommand(query, connection);
            int UserExist = (int)command.ExecuteScalar();
            connection.Close();
            if (UserExist > 0)
                return true;
            else
                return false;
        }

        public DataTable SelectByChatId(long ChatId)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From Pay where ChatId=" + ChatId;
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }
    }
}
