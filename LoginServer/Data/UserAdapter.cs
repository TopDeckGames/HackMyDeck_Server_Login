using System;
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
            cmd.CommandText = "SELECT id, username FROM user WHERE username = @login AND password = @password";
            cmd.Parameters.AddWithValue("@login", username);
            cmd.Parameters.AddWithValue("@password", password.Trim());
            MySqlDataReader reader = null;

            try
            {
                base.connection.Open();
                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    User user = new User((int)reader["id"]);
                    user.Login = (string)reader["username"];
                    return user;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                reader.Close();
                base.connection.Close();
            }

            return null;
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>0 : Echec, 1 : Ok, 2 : Login existant</returns>
        public short registration(User user)
        {
            MySqlCommand cmd = base.connection.CreateCommand();
            cmd.CommandText = "SELECT count(id) as nbId FROM user WHERE username = @login";
            cmd.Parameters.AddWithValue("@login", user.Login);
            MySqlDataReader reader = null;

            try
            {
                base.connection.Open();
                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    if ((int)reader["nbId"] != 0)
                        return 2;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                reader.Close();
                base.connection.Close();
            }

            cmd.CommandText = "INSERT INTO user(username, password, firstname, lastname) VALUES (@login, @password, @firstname, @lastname)";
            cmd.Parameters.AddWithValue("@login", user.Login);
            cmd.Parameters.AddWithValue("@password", user.Password.Trim());
            cmd.Parameters.AddWithValue("@firstname", user.Firstname);
            cmd.Parameters.AddWithValue("@lastname", user.Lastname);

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

            return 1;
        }
    }
}