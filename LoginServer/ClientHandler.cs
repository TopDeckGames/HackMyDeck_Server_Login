using System;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Net;

using LoginServer.Manager;

namespace LoginServer
{
    public class ClientHandler
    {
        private TcpClient tcpClient;
        private int messageLength;
        private Thread requestActionThread;
        private volatile NetworkStream clientStream;
        private volatile byte[] requests;
        private volatile bool active;
        public bool Active
        {
            get
            {
                return this.active;
            }
            set
            {
                if (!value)
                {
                    while (this.requests.Length > 0)
                    { //Tant qu'il reste des requêtes on ne désactive pas le client
                        continue;
                    }
                }
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
            this.requestActionThread = new Thread(new ThreadStart(requestAction));
            this.requestActionThread.Start();

        }

        /// <summary>
        /// Ecoute les requêtes du client
        /// </summary>
        public void handle()
        {
            if (this.tcpClient.Connected)
            {
                this.clientStream = this.tcpClient.GetStream();
                byte[] message = new byte[messageLength];
                int bytesRead;

                while (this.Active)
                {
                    bytesRead = 0;

                    try
                    {
                        bytesRead = clientStream.Read(message, 0, this.messageLength);
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

                    if (this.requests != null)
                    {
                        byte[] temp = new byte[this.requests.Length + message.Length];
                        this.requests.CopyTo(temp, 0);
                        message.CopyTo(temp, this.requests.Length);
                        this.requests = temp;
                    }
                    else
                    {
                        this.requests = message;
                    }
                }

                clientStream.Close();
                this.Active = false;
            }

            this.tcpClient.Close();
        }

        private void requestAction()
        {
            byte[] request = new byte[this.messageLength];


            while (this.active)
            {
                if (this.requests != null && this.requests.Length > 0 && this.requests.Length >= this.messageLength)
                {
                    for (int i = 0; i < this.messageLength; i++)
                    {
                        request[i] = (byte)this.requests.GetValue(i);
                    }

                    byte[] temp = new byte[this.requests.Length - this.messageLength];
                    for (int i = this.messageLength; i < this.requests.Length; i++)
                    {
                        temp[i - this.messageLength] = (byte)this.requests.GetValue(i);
                    }
                    this.requests = temp;

                    Logger.log(typeof(ClientHandler), "Requete", Logger.LogType.Info);
                    Stream stream = new MemoryStream(request);
                    var result = this.parser(stream);
                }
            }
        }

        private object parser(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    uint token = reader.ReadUInt32();
                    ushort dataSize = reader.ReadUInt16();
                    ushort idManager = reader.ReadUInt16();
                    char[] checksum = reader.ReadChars(32);

                    if (this.requests.Length < dataSize)
                    {
                        Logger.log(typeof(ClientHandler), "La requête est parvenue incomplète", Logger.LogType.Error);
                        return null;
                    }
                    else
                    {
                        byte[] message = new byte[dataSize];
                        for (ushort i = 0; i < dataSize; i++)
                        {
                            message[i] = (byte)this.requests.GetValue(i);
                        }

                        byte[] temp = new byte[this.requests.Length - dataSize];
                        for (int i = dataSize; i < this.requests.Length; i++)
                        {
                            temp[i - dataSize] = (byte)this.requests.GetValue(i);
                        }
                        this.requests = temp;

                        Stream dataStream = new MemoryStream(message);
                    }
                }
                catch (Exception e)
                {
                    Logger.log(typeof(ClientHandler), e.Message, Logger.LogType.Error);
                }
            }
            return new object();
        }
    }
}