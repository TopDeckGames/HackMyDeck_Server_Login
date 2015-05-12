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
        /// <returns>0 : Echec, 1 : Ok, 2 : Login existant</returns>
        public short registration(string login, string password, string email)
        {
            User user = new User(0);
            user.Login = login;
            user.Password = password;
            user.Email = email;

            try
            {
                return AdapterFactory.getUserAdapter().registration(user);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}