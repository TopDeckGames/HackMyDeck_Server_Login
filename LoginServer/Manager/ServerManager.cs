using System;
using System.Collections.Generic;
using LoginServer.Model;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Configuration;
using System.IO;

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
        /// Instancie la connexion vers un serveur précis
        /// </summary>
        /// <param name="server">Serveur auquel se connecter</param>
        public void connectToServer(Model.Server server)
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
                        catch(CryptographicException)
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
        public void disconnectFromServer()
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

                this.disconnectFromServer();
            }
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
