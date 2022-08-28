using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    class DownloadRepository : IDownloadRepository
    {
        string connectionString = constring.ConnectionString;

        public bool Insert(long ChatId, string ECode, string Type, string By, string UserType, DateTime Date,long Amount)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Insert Into [Download] (ChatId,ExamCode,[Type],[By],UserType,[Date],Amount) values (@ChatId,@ECode,@Type,@By,@UserType,@Date,@Amount)";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@ChatId", ChatId);
                command.Parameters.AddWithValue("@ECode", long.Parse(ECode));
                command.Parameters.AddWithValue("@Type", Type);
                command.Parameters.AddWithValue("@Date", By);
                command.Parameters.AddWithValue("@Type", UserType);
                command.Parameters.AddWithValue("@Date", Date.ToOADate());
                command.Parameters.AddWithValue("@Amount", Amount);
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

        public DataTable SelectAll()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From DownloadReport ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectByChatId(long ChatId)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From DownloadReport where ChatId=" + ChatId + " ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectByExam(long ExamCode)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From DownloadReport where Code=" + ExamCode + " ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectTop()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select top 10 * From DownloadReport ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }
    }
}
