using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    interface ITransactionsRepository
    {
        bool TransactionExist(long Id);
        DataTable SelectAll();
        DataTable SelectByChatId(long ChatId);
        bool Insert(long Amount, long ChatId, long TrackingCode, string About, DateTime Date, string Description);
        DataTable SelectTop();
        bool Delete(long Id);
    }
}
