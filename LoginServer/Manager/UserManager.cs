using System;
using System.IO;
using MySql.Data.MySqlClient;
using LoginServer.Model;
using LoginServer.Data;

namespace LoginServer.Manager
{
    public class UserManager
    {
        /// <summary>
        /// Récupère un utilisateur à partir de ses identifiants
        /// </summary>
        /// <returns>Un utilisteur</returns>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        public User getUser(string login, string password)
        {
            try
            {
                return AdapterFactory.getUserAdapter().connection(login, password);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Créer un nouvel utilisateur et l'enregistre dans la base
        /// </summary>
        /// <param name="login">Login.</param>
        /// <param name="password">Password.</param>
        public void registration(string login, string password)
        {
            User user = new User(0, login, password);

            try
            {
                AdapterFactory.getUserAdapter().registration(user);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}