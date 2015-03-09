using System;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;

namespace LoginServer
{
    class MonitorHandler
    {
        private TcpClient tcpClient;
        private int messageLength;
        private bool active;
        private RSACryptoServiceProvider rsaClient, rsaServer;
        
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
                //On génère les objets nécessaires à l'encryption
                this.rsaClient = new RSACryptoServiceProvider(1024);
                this.rsaServer = new RSACryptoServiceProvider(1024);
                this.rsaClient.FromXmlString(ConfigurationManager.AppSettings["monitoring_public_key"]);

                ASCIIEncoding byteConverter = new ASCIIEncoding();

                //On crypte la clef publique générée et on l'envoi au client
                String publicKeyServer = this.rsaServer.ToXmlString(false);
                List<byte[]> messages = SplitByteArray(byteConverter.GetBytes(publicKeyServer), 117);
                foreach (byte[] m in messages)
                {
                    this.tcpClient.Client.Send(this.rsaClient.Encrypt(m, false));
                }

            }
            catch(CryptographicException e)
            {
                Logger.log(typeof(MonitorHandler), "Erreur lors de la génération des clefs RSA : " + e.Message, Logger.LogType.Error);
                this.rsaClient.PersistKeyInCsp = false;
                this.rsaServer.PersistKeyInCsp = false;
            }
        }

        private static List<byte[]> SplitByteArray(byte[] array, int length)
        {
            int arrayLength = array.Length;
            List<byte[]> splitted = new List<byte[]>();

            for (int i = 0; i < arrayLength; i = i + length)
            {
                byte[] val = new byte[length];

                if (arrayLength < i + length)
                {
                    length = arrayLength - i;
                }
                Array.Copy(array, i, val, 0, length);
                splitted.Add(val);
            }

            return splitted;
        }
    }
}
