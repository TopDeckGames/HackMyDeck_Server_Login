using System;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace LoginServer.Data
{
    public abstract class BaseAdapter
    {
        protected string connectionString;
        protected MySqlConnection connection;

        public BaseAdapter()
        {
            this.connectionString = ConfigurationManager.ConnectionStrings["Connexion2"].ConnectionString;
            this.connection = new MySqlConnection(this.connectionString);
        }
    }
}