using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    class MessagesRepository : IMessagesRepository
    {
        string connectionString = constring.ConnectionString;

        public bool Insert(long SChatId, long RChatId, int MessageId, string Message, DateTime Date, int ReplyId, string Type)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Insert Into Messages (SChatId,RChatId,MessageId,Message,[Date],ReplyId,Type) VALUES (@SChatId,@RChatId,@MessageId,@Message,@Date,@ReplyId,@Type)";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@SChatId", SChatId);
                command.Parameters.AddWithValue("@RChatId", RChatId);
                command.Parameters.AddWithValue("@MessageId", MessageId);
                command.Parameters.AddWithValue("@Message", Message);
                command.Parameters.AddWithValue("@Date", Date.ToOADate());
                command.Parameters.AddWithValue("@ReplyId", ReplyId);
                command.Parameters.AddWithValue("@Type", Type);
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
            string query = "Select * From Messages ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectByChatId(long ChatId)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From Messages Where SChatId=" + ChatId + "or RChatId=" + ChatId + " ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectTop()
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "SELECT top 10 * FROM Messages ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }
    }
}
