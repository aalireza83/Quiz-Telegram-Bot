using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    interface IMessagesRepository
    {
        DataTable SelectAll();
        DataTable SelectByChatId(long ChatId);
        bool Insert(long SChatId, long RChatId, int MessageId, string Message, DateTime Date,int ReplyId,string Type);
        DataTable SelectTop();
    }
}
