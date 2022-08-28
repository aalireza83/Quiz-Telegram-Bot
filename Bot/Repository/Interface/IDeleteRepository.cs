using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    interface IDeleteRepository
    {
        DataTable SelectAll();
        bool Insert(long ExamCode,long ChatId, long MessageId);
        bool RemoveAll();
        bool DeleteExists();
        DataTable SelectByDate(DateTime Date);
        DataTable SelectDate();
        bool RemoveByDate(DateTime Date);
    }
}
