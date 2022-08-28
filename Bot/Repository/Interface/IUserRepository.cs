using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    interface IUserRepository
    {
        DataTable SelectAll();
        DataTable SelectRow(long ChatId);
        DataTable SelectByPanel(string Panel);
        bool Insert(long ChatId, string Name, string Panel, DateTime Date, string Phone, string LastCommand, long Amount);
        bool Delete(long ChatId);
        bool UpdatePanel(long ChatId, string Panel);
        bool UserExists(long ChatId);
        bool ChangeName(long ChatId, string Name);
        bool ChangePhone(long ChatId, string Phone);
        bool Updatelastcommand(long ChatId, string LastCommand);
        bool UpdateAmount(long ChatId, long Amount);
    }
}
