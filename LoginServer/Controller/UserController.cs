using System;
using System.IO;
using LoginServer.Model;
using LoginServer.Manager;

namespace LoginServer.Controller
{
    public class UserController : IController
    {
        /// <summary>
        /// Redirige la requête vers l'action correspondante
        /// </summary>
        /// <param name="stream">Flux de données à traiter</param>
        public Response parser(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    ushort idAction = reader.ReadUInt16();

                    switch (idAction)
                    {
                        case 1:
                            this.loginAction(
                                new string(reader.ReadChars(User.LOGIN_LENGTH)),
                                new string(reader.ReadChars(User.PASSWORD_LENGTH))
                            );
                            break;
                        case 2:
                            this.loginAction(
                                new string(reader.ReadChars(User.LOGIN_LENGTH)),
                                new string(reader.ReadChars(User.PASSWORD_LENGTH))
                            );
                            break;
                        default:
                            Logger.log(typeof(UserManager), "L'action n'existe pas : " + idAction, Logger.LogType.Error);
                            //Todo reponse erreur
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logger.log(typeof(UserController), e.Message, Logger.LogType.Error);
                }
            }
            return (Response)new object();
        }

        /// <summary>
        /// Essai de connecter l'utilisateur
        /// </summary>
        private void loginAction(string login, string password)
        {
            User user = null;

            try
            {
                user = ManagerFactory.getUserManager().getUser(login, password);
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
        /// Enregistre un nouvel utilisateur
        /// </summary>
        private void registerAction(string login, string password)
        {
            try
            {
                ManagerFactory.getUserManager().registration(login, password);
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