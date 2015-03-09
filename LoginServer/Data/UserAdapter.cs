﻿using System;
using MySql.Data.MySqlClient;
using LoginServer.Model;
using System.Data;

namespace LoginServer.Data
{
    public class UserAdapter : BaseAdapter
    {
        public UserAdapter()
            : base()
        {
        }

        /// <summary>
        /// Teste la validité des identifiants entrés par l'utilisateur
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public new User connection(string username, string password)
        {
            MySqlCommand cmd = base.connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM USER WHERE login = @login AND password = @password";
            cmd.Parameters.AddWithValue("@login", username);
            cmd.Parameters.AddWithValue("@password", password);
            MySqlDataReader reader;

            try
            {
                base.connection.Open();
                reader = cmd.ExecuteReader();
            }
            catch
            {
                throw;
            }
            finally
            {
                base.connection.Close();
            }

            if (reader.HasRows)
            {
                DataTable dt = new DataTable();
                dt.Load(reader);
                reader.Close();

                if (dt.Rows.Count == 1)
                {
                    DataRow row = dt.Rows[0];
                    User user = new User(int.Parse(row["id"].ToString()), row["login"].ToString(), null);
                    return user;
                }
            }

            reader.Close();
            return null;
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur
        /// </summary>
        /// <param name="user">User.</param>
        public void registration(User user)
        {
            MySqlCommand cmd = base.connection.CreateCommand();
            cmd.CommandText = "INSERT INTO USER(login, password) VALUES (@login, @password)";
            cmd.Parameters.AddWithValue("@login", user.Login);
            cmd.Parameters.AddWithValue("@password", user.Password);

            try
            {
                base.connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            finally
            {
                base.connection.Close();
            }
        }
    }
}