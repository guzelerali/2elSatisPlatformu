
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
namespace ReStore.Config
{
    public class DbConfig
    {
        private static readonly string server = "localhost";
        private static readonly string database = "restore";
        private static readonly string user = "root";
        private static readonly string password = ""; 

        public static string ConnectionString =>
            $@"Server={server};Database={database};User={user};Password={password};SslMode=Disabled;";
       
        public static string DatabaseName => database;
    }
}