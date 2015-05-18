using System;
using System.Linq;
using System.Collections.Generic;
using LoginServer.Model;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Configuration;
using System.IO;
using System.Threading;
using LoginServer.Helper;

namespace LoginServer
{
    public class ServerManager
    {
        private List<Model.Server> servers = new List<Model.Server>();
        private TcpClient tcpClient;
        private RSACryptoServiceProvider rsaClient, rsaServer;

        /// <summary>
        /// Ajoute un serveur à la liste des serveurs disponibles
        /// </summary>
        /// <param name="server"></param>
        public void addServer(Model.Server server)
        {
            List<Model.Server> temp = new List<Model.Server>();
            foreach (Model.Server s in this.servers)
            {
                if (!s.Name.Equals(server.Name))
                {
                    temp.Add(s);
                }
            }
            this.servers = temp;
            this.servers.Add(server);
        }

        /// <summary>
        /// Retourne la liste de tous les serveurs disponibles
        /// </summary>
        /// <returns>Liste de serveurs</returns>
        public List<Model.Server> getServers()
        {
            return this.servers;
        }

        /// <summary>
        /// Récupère un serveur à partir de son nom
        /// </summary>
        /// <param name="name">Nom du serveur</param>
        /// <returns></returns>
        public Model.Server getServer(string name)
        {
            return (Model.Server)this.servers.Where(x => x.Name == name);
        }

        /// <summary>
        /// Instancie la connexion vers un serveur précis
        /// </summary>
        /// <param name="server">Serveur auquel se connecter</param>
        private void connectToServer(Model.Server server)
        {
            try
            {
                //Ouverture de la connexion
                this.tcpClient = new TcpClient();
                this.tcpClient.Connect(server.Address, server.PortMonitoring);

                //Récupèration de la clef de chiffrement
                StreamReader keyFile = new StreamReader(ConfigurationManager.AppSettings["server_private_key"]);
                string key = keyFile.ReadToEnd();
                keyFile.Close();

                NetworkStream stm = this.tcpClient.GetStream();
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] message = new byte[1024];

                try
                {
                    this.rsaClient = new RSACryptoServiceProvider(1024);
                    this.rsaServer = new RSACryptoServiceProvider(1024);
                    this.rsaClient.FromXmlString(key);

                    //Récupèration de la clef publique de l'autre pair
                    string serverKey = "";
                    bool flag = true;
                    int count = 0;
                    while (flag && count < 10) //Tant que la clef n'est pas valide et que l'on a reçu moins de 10 messages
                    {
                        count++;
                        int size = stm.Read(message, 0, 1024);
                        byte[] data = new byte[size];
                        for (int i = 0; i < size; i++)
                            data[i] = message[i];

                        serverKey += asen.GetString(rsaClient.Decrypt(data, false));
                        try
                        {
                            this.rsaServer.FromXmlString(serverKey);
                            flag = false;
                        }
                        catch(Exception)
                        {
                            flag = true;
                        }
                    }
                }
                catch (CryptographicException e)
                {
                    Logger.log(typeof(ServerManager), "L'encryption des données a échoué : " + e.Message, Logger.LogType.Error);
                    this.disconnectFromServer();
                }
            }
            catch (SocketException e)
            {
                Logger.log(typeof(ServerManager), "La connexion au serveur a échoué " + e.Message, Logger.LogType.Error);
                this.disconnectFromServer();
            }
            catch (IOException e)
            {
                Logger.log(typeof(ServerManager), "La lecture du fichier xml a échoué " + e.Message, Logger.LogType.Error);
                this.disconnectFromServer();
            }
        }

        /// <summary>
        /// Déconnexion du serveur courrant
        /// </summary>
        private void disconnectFromServer()
        {
            if(this.rsaClient != null)
                this.rsaClient.PersistKeyInCsp = false;
            if(this.rsaServer != null)
                this.rsaServer.PersistKeyInCsp = false;
            if(this.tcpClient != null)
                this.tcpClient.Close();
        }

        /// <summary>
        /// Teste l'état de chacun des serveurs enregistrés
        /// </summary>
        public void checkServers()
        {
            foreach (Model.Server server in this.servers)
            {
                this.connectToServer(server);

                try
                {
                    if (this.tcpClient.Connected)
                    {
                        //Préparation de la requête
                        Request req = new Request();
                        req.Type = Request.TypeRequest.Check;
                        string reqJson = JsonSerializer.toJson(req);

                        //Envoi de la requête
                        this.sendToServer(reqJson);
                        Request response = this.waitResponseFromServer();
                        server.Available = true;

                        this.disconnectFromServer();
                    }
                    else
                    {
                        server.Available = false;
                    }
                }
                catch (Exception)
                {
                    server.Available = false;
                }

                if(server.Available)
                    Logger.log(typeof(ServerManager), String.Format("Serveur {0}({1}) disponible", server.Name, server.Type), Logger.LogType.Info);
                else
                    Logger.log(typeof(ServerManager), String.Format("Serveur {0}({1}) indisponible", server.Name, server.Type), Logger.LogType.Info);
            }
        }

        /// <summary>
        /// Inscrit le joueur dans un serveur de gestion
        /// </summary>
        /// <param name="user">Utilisateur à inscrire dans le serveur</param>
        /// <returns>Serveur sélectionné</returns>
        public Model.Server enterInServer(User user)
        {
            Model.Server selected = null;
            //On sélectionne un serveur parmis la liste
            foreach (Model.Server server in this.servers)
            {
                //Si le serveur est un serveur de gestion disponible et qui n'a pas atteind sa capacité maximale
                if (server.Type.Equals(Model.Server.ServerType.Gestion) && server.Available && server.NbPlayers < server.MaxPlayers)
                {
                    //Si le serveur est moins chargé que celui sélectionné on échange pour équilibrer les charges
                    if (selected == null || server.NbPlayers < selected.NbPlayers)
                    {
                        selected = server;
                    }
                }
            }

            //Si un serveur a été sélectionné on le contact pour enregistrer le joueur
            if (selected != null)
            {
                this.connectToServer(selected);

                //Préparation de la requête
                Request req = new Request();
                req.Type = Request.TypeRequest.Register;
                req.Data = JsonSerializer.toJson(user);
                string message = JsonSerializer.toJson(req);
                //Envoi de la requête
                this.sendToServer(message);
                this.waitResponseFromServer();
                this.disconnectFromServer();
            }
            return selected;
        }

        /// <summary>
        /// Enregistre un combat dans un serveur disponible
        /// </summary>
        /// <param name="combat">Contient les informations du combat</param>
        /// <returns>Serveur sélectionné</returns>
        public Model.Server registerCombat(Combat combat)
        {
            Model.Server selected = null;
            //On sélectionne un serveur parmis la liste
            foreach (Model.Server server in this.servers)
            {
                //Si le serveur est un serveur de combat disponible et qui n'a pas atteind sa capacité maximale
                if (server.Type.Equals(Model.Server.ServerType.Combat) && server.Available && server.NbPlayers < server.MaxPlayers)
                {
                    //Si le serveur est moins chargé que celui sélectionné on échange pour équilibrer les charges
                    if (selected == null || server.NbPlayers < selected.NbPlayers)
                    {
                        selected = server;
                    }
                }
            }

            //Si un serveur a été sélectionné on le contact pour enregistrer le joueur
            if (selected != null)
            {
                this.connectToServer(selected);

                //Préparation de la requête
                Request req = new Request();
                req.Type = Request.TypeRequest.Register;
                req.Data = JsonSerializer.toJson(combat);
                string message = JsonSerializer.toJson(req);
                //Envoi de la requête
                this.sendToServer(message);
                this.waitResponseFromServer();
                this.disconnectFromServer();
            }
            return selected;
        }

        /// <summary>
        /// Annonce au serveur de gestion qu'un combat a été trouvé
        /// </summary>
        /// <param name="server">Serveur de gestion à contacter</param>
        /// <param name="user">Joueur concerné par le combat</param>
        /// <param name="selected">Serveur de combat vers lequel rediriger le joueur</param>
        public void registerCombat(Model.Server server, User user, Model.Server selected)
        {
            this.connectToServer(server);

            //Préparation de la requête
            Request req = new Request();
            req.Type = Request.TypeRequest.EnterCombat;
            KeyValuePair<User, Model.Server> data = new KeyValuePair<User, Model.Server>(user, selected);
            req.Data = JsonSerializer.toJson(data);
            string message = JsonSerializer.toJson(req);
            //Envoi de la requête
            this.sendToServer(message);
            this.waitResponseFromServer();
            this.disconnectFromServer();
        }

        /// <summary>
        /// Envoi une chaine de charactères au serveur en la cryptant
        /// </summary>
        /// <param name="message">Message à envoyer</param>
        private void sendToServer(string message)
        {
            ASCIIEncoding byteConverter = new ASCIIEncoding();

            List<byte[]> messages = ByteArray.SplitByteArray(byteConverter.GetBytes(message), 117);
            foreach (byte[] m in messages)
            {
                this.tcpClient.Client.Send(this.rsaServer.Encrypt(m, false));
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Attend une réponse du serveur distant
        /// </summary>
        /// <returns>Request</returns>
        private Request waitResponseFromServer()
        {
            NetworkStream clientStream = this.tcpClient.GetStream();
            int messageLength = int.Parse(ConfigurationManager.AppSettings["monitoring_message_length"]);
            byte[] message = new byte[messageLength];
            int bytesRead;
            ASCIIEncoding byteConverter = new ASCIIEncoding();
            Request req = null;
            StringBuilder requests = new StringBuilder();

            while (req == null)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = clientStream.Read(message, 0, messageLength);
                }
                catch (Exception e)
                {
                    Logger.log(typeof(ServerManager), "Erreur lors de l'execution du socket : " + e.Message, Logger.LogType.Error);
                    break;
                }

                if (bytesRead == 0)
                {
                    Logger.log(typeof(ServerManager), "Connexion interrompue", Logger.LogType.Info);
                    break;
                }

                //On récupère le message exact qui a été reçu
                byte[] data = new byte[bytesRead];
                for (int i = 0; i < bytesRead; i++)
                    data[i] = message[i];

                try
                {
                    //On décrypte la chaine charactère et on l'ajoute à la pile
                    string request = byteConverter.GetString(this.rsaClient.Decrypt(data, false));
                    requests.Append(request);
                    req = JsonSerializer.fromJson<Request>(requests.ToString());
                }
                catch (CryptographicException e)
                {
                    Logger.log(typeof(ServerManager), "Erreur lors du décryptage du message : " + e.Message, Logger.LogType.Error);
                }
                catch
                {

                }
            }

            return req;
        }

        /// <summary>
        /// Information sur les serveurs enregistrés
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Model.Server server in this.servers)
            {
                builder.AppendLine(String.Format("{0}({4}) - {1} [{2}, {5}] - {3} joueurs max", server.Name, server.Address.ToString(), server.Port, server.MaxPlayers, server.Type, server.PortMonitoring));
            }
            return builder.ToString();
        }
    }
}
