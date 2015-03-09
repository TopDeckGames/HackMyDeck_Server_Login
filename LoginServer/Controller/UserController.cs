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
        	Response response;
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    ushort idAction = reader.ReadUInt16();

                    switch (idAction)
                    {
                        case 1:
                            response = this.loginAction(
                                new string(reader.ReadChars(User.LOGIN_LENGTH)),
                                new string(reader.ReadChars(User.PASSWORD_LENGTH))
                            );
                    		response.addValue(1);
                            break;
                        case 2:
                            response = this.registerAction(
                                new string(reader.ReadChars(User.LOGIN_LENGTH)),
                                new string(reader.ReadChars(User.PASSWORD_LENGTH))
                            );
                            response.addValue(1);
                            break;
                        default:
                            Logger.log(typeof(UserManager), "L'action n'existe pas : " + idAction, Logger.LogType.Error);
                            response = new Response();
                            response.addValue(0);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logger.log(typeof(UserController), e.Message, Logger.LogType.Error);
                    response = new Response();
                    response.addValue(0);
                }
            }
            return response;
        }

        /// <summary>
        /// Essai de connecter l'utilisateur
        /// </summary>
        private Response loginAction(string login, string password)
        {
            User user = null;
            Response response = new Response();

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
            	response.addValue(0);
            }
            else
            {
            	//Ajout des valeurs de l'utilisateur
            	response.addValue(user.Id);
            	//Ajout des données du serveur de jeu
                //Todo inscription dans un serveur libre
                
                //Ajout de l'ID de la réponse
                response.addValue(1);
            }
            
            return response;
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur
        /// </summary>
        private Response registerAction(string login, string password)
        {return new Response();
            try
            {
                ManagerFactory.getUserManager().registration(login, password);
            }
            catch (Exception e)
            {
                Logger.log(typeof(UserManager), "Impossible d'enregistrer l'utilisateur : " + e.Message, Logger.LogType.Error);
                //Todo construire réponse erreur
                return new Response();
            }

            //Todo construire réponse succés
        }
    }
}