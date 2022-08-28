using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    interface IQuestionsRepository
    {
        DataTable SelectByExamCode(long ExamCode);
        bool Insert(long ExamCode, int QuestionNumber, string Question, string Answer, int Key, int HardLevel);
        bool Delete(long ExamCode);
    }
}
