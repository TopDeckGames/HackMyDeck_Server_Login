using System;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using LoginServer.Helper;
using LoginServer.Model;
using System.Diagnostics;

namespace LoginServer.Handlers
{
    class MonitorHandler
    {
        private TcpClient tcpClient;
        private int messageLength;
        private bool active;
        private RSACryptoServiceProvider rsaClient, rsaServer;
        private NetworkStream clientStream;
        private volatile StringBuilder requests = new StringBuilder();
        private Thread handleThread;
        
        /// <summary>
        /// Initialise le MonitorHandler
        /// </summary>
        /// <param name="client">Client TCP</param>
        public MonitorHandler(object client)
        {
            this.tcpClient = (TcpClient)client;

            if (this.tcpClient.Connected)
            {
                Logger.log(typeof(MonitorHandler), "Client de monitoring connecté : " + ((IPEndPoint)this.tcpClient.Client.RemoteEndPoint).Address.ToString(), Logger.LogType.Info);
            }
            else
            {
                throw new Exception("Client non connecté");
            }

            //Récupèration de la taille des blocs
            this.messageLength = int.Parse(ConfigurationManager.AppSettings["monitoring_message_length"]);
            this.active = true;

            try
            {
                //Récupèration de la clef de chiffrement
                StreamReader keyFile = new StreamReader(ConfigurationManager.AppSettings["monitoring_public_key"]);
                string key = keyFile.ReadToEnd();
                keyFile.Close();

                //On génère les objets nécessaires à l'encryption
                this.rsaClient = new RSACryptoServiceProvider(1024);
                this.rsaServer = new RSACryptoServiceProvider(1024);
                this.rsaClient.FromXmlString(key);

                ASCIIEncoding byteConverter = new ASCIIEncoding();

                //On crypte la clef publique générée et on l'envoi au client
                String publicKeyServer = this.rsaServer.ToXmlString(false);
                List<byte[]> messages = ByteArray.SplitByteArray(byteConverter.GetBytes(publicKeyServer), 117);
                foreach (byte[] m in messages)
                {
                    this.tcpClient.Client.Send(this.rsaClient.Encrypt(m, false));
                    Thread.Sleep(50);
                }

            }
            catch(CryptographicException e)
            {
                Logger.log(typeof(MonitorHandler), "Erreur lors de la génération des clefs RSA : " + e.Message, Logger.LogType.Error);
                this.rsaClient.PersistKeyInCsp = false;
                this.rsaServer.PersistKeyInCsp = false;
            }

            this.handleThread = new Thread(new ThreadStart(handle));
            this.handleThread.Start();
        }

        /// <summary>
        /// Ecoute les requêtes du client
        /// </summary>
        public void handle()
        {
            if (this.tcpClient.Connected)
            {
                this.clientStream = this.tcpClient.GetStream();
                byte[] message = new byte[this.messageLength];
                int bytesRead;
                ASCIIEncoding byteConverter = new ASCIIEncoding();

                while (this.active)
                {
                    bytesRead = 0;

                    try
                    {
                        bytesRead = clientStream.Read(message, 0, this.messageLength);
                    }
                    catch (Exception e)
                    {
                        Logger.log(typeof(MonitorHandler), "Erreur lors de l'execution du socket : " + e.Message, Logger.LogType.Error);
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        Logger.log(typeof(MonitorHandler), "Connexion interrompue", Logger.LogType.Info);
                        break;
                    }

                    //On récupère le message exact qui a été reçu
                    byte[] data = new byte[bytesRead];
                    for (int i = 0; i < bytesRead; i++)
                        data[i] = message[i];

                    try
                    {
                        //On décrypte la chaine charactère et on l'ajoute à la pile
                        string request = byteConverter.GetString(this.rsaServer.Decrypt(data, false));
                        this.requests.Append(request);
                    }
                    catch (CryptographicException e)
                    {
                        Logger.log(typeof(MonitorHandler), "Erreur lors du décryptage du message : " + e.Message, Logger.LogType.Error);
                    }
                }

                clientStream.Close();
                this.active = false;
            }

            this.tcpClient.Close();
        }

        public void analyser()
        {
            while (this.active || this.requests.Length > 0)
            {
                Request req;
                string response = String.Empty;
                try
                {
                    if ((req = JsonSerializer.fromJson<Request>(this.requests.ToString())) == null)
                    {
                        continue;
                    }
                    this.requests.Clear();
                }
                catch (Exception)
                {
                    continue;
                }

                Request rep = new Request();
                switch (req.Type)
                {
                    case Request.TypeRequest.Check:
                        Dictionary<string, object> temp = new Dictionary<string, object>();
                        Process proc = Process.GetCurrentProcess();
                        temp.Add("memory", proc.PrivateMemorySize64);
                        temp.Add("nbPlayers", MainClass.Server.getNbPlayers());
                        rep.Type = Request.TypeRequest.Response;
                        rep.Data = JsonSerializer.toJson(temp);
                        response = JsonSerializer.toJson(rep);
                        break;
                    case Request.TypeRequest.EnterCombat:
                        KeyValuePair<User, Object> data = JsonSerializer.fromJson<KeyValuePair<User, Object>>(req.Data);
                        MainClass.CombatQueue.addInQueue(data);
                        break;
                    case Request.TypeRequest.LeaveCombat:
                        User user = JsonSerializer.fromJson<User>(req.Data);
                        MainClass.CombatQueue.removeFromQueue(user);
                        break;
                    default:
                        continue;
                }

                this.sendMessage(response);
            }
        }

        /// <summary>
        /// Encrypte un message et l'envoi au serveur
        /// </summary>
        /// <param name="message">Chaine de charactères à envoyer</param>
        public void sendMessage(string message)
        {
            ASCIIEncoding byteConverter = new ASCIIEncoding();
            List<byte[]> messages = ByteArray.SplitByteArray(byteConverter.GetBytes(message), 117);
            foreach (byte[] m in messages)
            {
                this.tcpClient.Client.Send(this.rsaClient.Encrypt(m, false));
                Thread.Sleep(50);
            }
        }
    }
}