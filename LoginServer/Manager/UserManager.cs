using System;
using System.IO;
using MySql.Data.MySqlClient;
using LoginServer.Model;
using LoginServer.Data;

namespace LoginServer.Manager
{
    public class UserManager : IManager
    {
        public UserManager()
            : base()
        {
        }

        public Response parser(BinaryReader reader)
        {
            this.connection("test", "test");
            return null;
        }

        /// <summary>
        /// Connecte l'utilisateur avec les données de connexion données
        /// </summary>
        /// <param name="login">Login.</param>
        /// <param name="password">Password.</param>
        private void connection(string login, string password)
        {
            User user = null;

            try
            {
                user = AdapterManager.getUserAdapter().connection(login, password);
            }
            catch (Exception e)
            {
                Logger.log(typeof(UserManager), "Impossible de connecter l'utilisateur : " + e.Message, Logger.LogType.Error);
            }

            if (user == null)
            {
                //Todo construire réponse erreur
            }
            else
            {
                //Todo inscription dans un serveur libre
                //Todo construire réponse succés
            }
        }

        /// <summary>
        /// Créer un nouvel utilisateur et l'enregistre dans la base
        /// </summary>
        /// <param name="login">Login.</param>
        /// <param name="password">Password.</param>
        private void registration(string login, string password)
        {
            User user = new User(0, login, password);

            try
            {
                AdapterManager.getUserAdapter().registration(user);
            }
            catch (Exception e)
            {
                Logger.log(typeof(UserManager), "Impossible d'enregistrer l'utilisateur : " + e.Message, Logger.LogType.Error);
                //Todo construire réponse erreur
                return;
            }

            //Todo construire réponse succés
        }
    }
}