using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    interface ISettingsRepository
    {
        DataTable SelectSettings();
        bool UpdateSettings(bool stopbot, bool pused, bool QuestionByQuestion, bool AnswerSheet, string FileId, bool AutoBackUp);
    }
}
