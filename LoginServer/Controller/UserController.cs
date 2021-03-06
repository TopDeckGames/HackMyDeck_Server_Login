﻿using System;
using System.IO;
using LoginServer.Model;
using LoginServer.Manager;
using LoginServer.Helper;

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
                                StringHelper.getTrueString(new string(reader.ReadChars(User.LOGIN_LENGTH))),
                                new string(reader.ReadChars(User.PASSWORD_LENGTH))
                            );
                            break;
                        case 2:
                            response = this.registerAction(
                                StringHelper.getTrueString(new string(reader.ReadChars(User.LOGIN_LENGTH))),
                                System.Text.Encoding.UTF8.GetString(reader.ReadBytes(User.PASSWORD_LENGTH)),
                                StringHelper.getTrueString(new string(reader.ReadChars(User.EMAIL_LENGTH)))
                            );
                            break;
                        default:
                            Logger.log(typeof(UserController), "L'action n'existe pas : " + idAction, Logger.LogType.Error);
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
            response.openWriter();

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
                response.addValue(1);

            	//Ajout des valeurs de l'utilisateur
            	response.addValue(user.Id);

                //Inscription dans un serveur disponible
                ManagerFactory.getServerManager().checkServers();
                Model.Server server = ManagerFactory.getServerManager().enterInServer(user);

                if(server != null)
                {
                    response.addValue((ushort)1);
                    //Ajout des données de connexion au server
                    response.addValue(BitConverter.ToInt32(server.Address.GetAddressBytes(), 0));
                    response.addValue(server.Port); 
                }
                else
                {
                    response.addValue((ushort)0);
                }
            }
            
            return response;
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur
        /// </summary>
        private Response registerAction(string login, string password, string email)
        {
            Response response = new Response();
            response.openWriter();

            try
            {
                short state = ManagerFactory.getUserManager().registration(login, password, email);
                response.addValue(1);
                response.addValue(state);
            }
            catch (Exception e)
            {
                Logger.log(typeof(UserManager), "Impossible d'enregistrer l'utilisateur : " + e.Message, Logger.LogType.Error);
                response.addValue(0);
            }

            return response;
        }
    }
}