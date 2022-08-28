using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    interface IExamAccessRepository
    {
        bool Insert(long ChatId, long ExamCode);
        bool Delete(long ChatId, long ExamCode);
        DataTable SelectByChatId(long ChatId);
        bool Exist(long ChatId, long ExamCode);
        DataTable SelectByExam(long ExamCode);
        DataTable SelectTop();
        DataTable SelectAll();
    }
}
