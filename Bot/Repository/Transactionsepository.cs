using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    class TransactionsRepository : ITransactionsRepository
    {
        string connectionString = constring.ConnectionString;
        public bool Delete(long Id)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Delete From Transactions where ID=" + Id;
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

        public bool Insert(long Amount, long ChatId, long TrackingCode, string About, DateTime Date, string Description)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Insert Into Transactions (Amount,ChatId,TrackingCode,About,[Date],Description) values (@Amount,@ChatId,@TrackingCode,@About,@Date,@Description)";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@Amount", Amount);
                command.Parameters.AddWithValue("@ChatId", ChatId);
                command.Parameters.AddWithValue("@TrackingCode", TrackingCode);
                command.Parameters.AddWithValue("@About", About);
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
            string query = "Select * From Transactions ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectByChatId(long ChatId)
        {
                OleDbConnection connection = new OleDbConnection(connectionString);
                string query = "Select * From Transactions where ChatId=" + ChatId + " ORDER BY Date DESC";
                OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                DataTable data = new DataTable();
                adapter.Fill(data);
                return data;
        }

        public DataTable SelectTop()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "SELECT top 10 * FROM Transactions ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public bool TransactionExist(long Id)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "SELECT COUNT(*) FROM Transactions WHERE ID=" + Id;
            connection.Open();
            OleDbCommand command = new OleDbCommand(query, connection);
            int UserExist = (int)command.ExecuteScalar();
            connection.Close();
            if (UserExist > 0)
                return true;
            else
                return false;
        }
    }
}
