using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    interface IDownloadRepository
    {
        DataTable SelectAll();
        DataTable SelectByChatId(long ChatId);
        bool Insert(long ChatId, string ExamCode, string Type, string By, string UserType, DateTime Date, long Amount);
        DataTable SelectByExam(long ExamCode);
        DataTable SelectTop();
    }
}
