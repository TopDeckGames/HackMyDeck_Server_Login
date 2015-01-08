using System;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.IO;
using System.Configuration;
using System.Net;

using LoginServer.Manager;

namespace LoginServer
{
    public class ClientHandler
    {
        private TcpClient tcpClient;
        private int messageLength;
        private volatile bool active;
        public bool Active
        {
            get
            {
                return this.active;
            }
            set
            {
                this.active = value;
            }
        }

        /// <summary>
        /// Initialise le ClientHandler
        /// </summary>
        /// <param name="client">Client TCP</param>
        public ClientHandler(object client)
        {
            this.tcpClient = (TcpClient)client;

            if (this.tcpClient.Connected)
            {
                Logger.log(typeof(ClientHandler), "Client connecté : " + ((IPEndPoint)this.tcpClient.Client.RemoteEndPoint).Address.ToString(), Logger.LogType.Info);
            }
            else
            {
                throw new Exception("Client non connecté");
            }

            messageLength = int.Parse(ConfigurationManager.AppSettings["message_length"]);
            this.Active = true;
        }

        /// <summary>
        /// Ecoute les requêtes du client
        /// </summary>
        public void handle()
        {
            if (this.tcpClient.Connected)
            {
                NetworkStream clientStream = this.tcpClient.GetStream();
                byte[] message = new byte[messageLength];
                int bytesRead;

                while (this.Active)
                {
                    bytesRead = 0;

                    try
                    {
                        bytesRead = clientStream.Read(message, 0, messageLength);
                    }
                    catch (Exception e)
                    {
                        Logger.log(typeof(ClientHandler), "Erreur lors de l'execution du socket : " + e.Message, Logger.LogType.Error);
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        Logger.log(typeof(ClientHandler), "Connexion interrompue", Logger.LogType.Info);
                        break;
                    }

                    Stream stream = new MemoryStream(message);
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        var i = reader.ReadInt32();
                        var j = reader.ReadUInt32();

                        UserManager manager = new UserManager();
                        manager.parser(reader);
                    }
                }

                clientStream.Close();
                this.Active = false;
            }

            this.tcpClient.Close();
        }
    }
}