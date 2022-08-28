using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    interface IExamsRepository
    {
        DataTable SelectAll();
        DataTable SelectTop();
        DataTable SelectRow(long Code);
        bool Insert(long Code,string Name,DateTime Date,DateTime Time);
        bool Delete(long Code);
        bool UpdateTime(long Code, DateTime Time);
        bool ExamExist(long Code);
    }
}
