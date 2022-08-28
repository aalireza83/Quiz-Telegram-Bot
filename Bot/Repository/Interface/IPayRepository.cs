using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    interface IPayRepository
    {
        bool PayExist(long TrackingCode);
        DataTable SelectAll();
        DataTable Selectrow(long TrackingCode);
        DataTable SelectByChatId(long ChatId);
        bool Insert(long TrackingCode, long ChatId, long Amount, string UserName, string Name, string Phone, string CardNumber, DateTime Date, string Description);
        DataTable SelectTop();
        bool Delete(long TrackingCode);
    }
}
