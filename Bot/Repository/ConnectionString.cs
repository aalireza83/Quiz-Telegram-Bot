using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Repository
{
    static class constring
    {
        public static string ConnectionString = "Provider=Microsoft.ACE.OleDB.12.0;Data Source=" + Environment.CurrentDirectory + "\\Database.accdb;Jet OLEDB:Database Password=Am.15200518;";
    }
}
