using System;
using MySql.Data.MySqlClient;
using System.IO;

namespace LoginServer.Manager
{
    public abstract class BaseManager : IManager
    {
        protected string connectionString;
        protected MySqlConnection connection;

        public BaseManager()
        {
            this.connectionString = "SERVER=127.0.0.1; DATABASE=mli; UID=root; PASSWORD=";
            this.connection = new MySqlConnection(this.connectionString);
        }

        public abstract Response parser(BinaryReader reader);
    }
}