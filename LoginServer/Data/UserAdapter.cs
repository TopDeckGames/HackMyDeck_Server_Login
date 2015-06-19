using System;
using MySql.Data.MySqlClient;
using LoginServer.Model;
using System.Data;
using System.Collections.Generic;

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
            try
            {
                base.connection.Open();

                MySqlCommand cmd = base.connection.CreateCommand();
                cmd.CommandText = "SELECT count(id) as nbId FROM user WHERE username = @login";
                cmd.Parameters.AddWithValue("@login", user.Login);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (reader.GetOrdinal("nbId") != 0)
                            return 2;
                    }
                }

                cmd = base.connection.CreateCommand();
                cmd.CommandText = "INSERT INTO user(username, password, email, credit) VALUES (@login, @password, @email, @credit)";
                cmd.Parameters.AddWithValue("@login", user.Login);
                cmd.Parameters.AddWithValue("@password", user.Password.Trim());
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@credit", user.Credit);

                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT id FROM user WHERE username = @login AND password = @password";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        user.Id = (int)reader["id"];
                    }
                }

                cmd = base.connection.CreateCommand();
                cmd.CommandText = "SELECT id FROM structure";
                List<int> structures = new List<int>();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            structures.Add((int)reader["id"]);
                        }
                    }
                }

                foreach(int id in structures)
                {
                    MySqlCommand cmd2 = base.connection.CreateCommand();
                    cmd2.CommandText = "INSERT INTO user_structure(user_id, structure_id, effectif, level, locked) VALUES (@userId, @structureId, 0, 1, 0);";
                    cmd2.Parameters.AddWithValue("@userId", user.Id);
                    cmd2.Parameters.AddWithValue("@structureId", id);
                    cmd2.ExecuteNonQuery();
                }
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