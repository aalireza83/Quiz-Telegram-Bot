using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    class UserRepository : IUserRepository
    {
        string connectionString = constring.ConnectionString;
        public bool Delete(long ChatId)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Delete From Users where ChatId=" + ChatId;
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

        public bool Insert(long ChatId, string Name, string Panel, DateTime Date, string Phone, string LastCommand, long Amount)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Insert Into Users values (@ChatId,@Name,@Panel,@Date,@Phone,@LastCommand,@Amount)";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@ChatId", ChatId);
                command.Parameters.AddWithValue("@Name", Name);
                command.Parameters.AddWithValue("@Panel", Panel);
                command.Parameters.AddWithValue("@Date", Date.ToOADate());
                command.Parameters.AddWithValue("@Phone", Phone);
                command.Parameters.AddWithValue("@LastCommand", LastCommand);
                command.Parameters.AddWithValue("@Amount", Amount);
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
            string query = "Select * From [Users] ORDER BY [Date] DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectRow(long ChatId)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From Users Where ChatId=" + ChatId;
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public bool UserExists(long ChatId)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "SELECT COUNT(*) FROM Users WHERE ChatId=" + ChatId;
            connection.Open();
            OleDbCommand command = new OleDbCommand(query, connection);
            int UserExist = (int)command.ExecuteScalar();
            connection.Close();
            if (UserExist > 0)
                return true;
            else
                return false;
        }

        public bool UpdatePanel(long ChatId, string Panel)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Update Users Set Panel=@Panel Where ChatId=" + ChatId;
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@Panel", Panel);
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

        public bool ChangeName(long ChatId, string Name)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Update Users Set Name=@Name Where ChatId=" + ChatId;
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@Name", Name);
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

        public bool ChangePhone(long ChatId, string Phone)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Update Users Set  Phone=@Phone Where ChatId=" + ChatId;
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@Phone", Phone);
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

        public DataTable SelectByPanel(string Panel)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select * From Users Where Panel=" + '"' + Panel + '"' + " ORDER BY Date DESC";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public bool Updatelastcommand(long ChatId, string LastCommand)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                string query = "Update Users Set LastCommand=@LastCommand Where ChatId=" + ChatId;
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@LastCommand", LastCommand);
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

        public bool UpdateAmount(long ChatId, long Amount)
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            string query = "Select Amount From Users Where ChatId=" + ChatId;
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            long cash = long.Parse(data.Rows[0][0].ToString()) + Amount;
            OleDbConnection connection2 = new OleDbConnection(connectionString);
            try
            {
                string query2 = "Update Users Set Amount=@Amount Where ChatId=" + ChatId;
                OleDbCommand command = new OleDbCommand(query2, connection2);
                command.Parameters.AddWithValue("@Amount", cash);
                connection2.Open();
                command.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                connection2.Close();
            }
        }
    }
}
