using System;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Net;

using LoginServer.Controller;
using LoginServer.Helper;

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

            //Récupèration de la taille des blocs
            this.messageLength = int.Parse(ConfigurationManager.AppSettings["message_length"]);
            this.Active = true;
            //Démarage du thread de traitement des requêtes
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
                byte[] message = new byte[this.messageLength];
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

                    //Si la pile n'est pas nulle alors on empile
                    if (this.requests != null)
                    {
                        byte[] temp = new byte[this.requests.Length + message.Length];
                        this.requests.CopyTo(temp, 0);
                        message.CopyTo(temp, this.requests.Length);
                        this.requests = temp;
                    }
                    else //Sinon on initialise la pile
                    {
                        this.requests = new byte[message.Length];
                        message.CopyTo(this.requests, 0);
                    }
                }

                clientStream.Close();
                this.Active = false;
            }

            this.tcpClient.Close();
        }

        /// <summary>
        /// Traite les requêtes
        /// </summary>
        private void requestAction()
        {
            byte[] request;

            //Tant que le client est actif où qu'il reste des requêtes non traitées
            while (this.active || (this.requests != null && this.requests.Length > 0))
            {
                try
                {
                    request = this.getRequestPart();
                }
                catch
                {
                    continue;
                }

                Logger.log(typeof(ClientHandler), "Requete", Logger.LogType.Info);
                Stream stream = new MemoryStream(request);
                var result = this.parser(stream);
            }
        }

        /// <summary>
        /// Récupère plusieurs blocs de requête dans la pile
        /// </summary>
        /// <returns>Blocs de requête</returns>
        /// <param name="nb">Nombre de blocs à récupérer</param>
        private byte[] getRequestPart(int nb = 1)
        {
            byte[] request = new byte[this.messageLength * nb];

            //Si la pile contient assez de données
            if (this.requests != null)
            {
                if (this.requests.Length >= this.messageLength * nb)
                {
                    //On récupère les x premiers blocs
                    for (int i = 0; i < this.messageLength * nb; i++)
                    {
                        request[i] = (byte)this.requests.GetValue(i);
                    }

                    //On enlève les blocs récupérés de la pile
                    byte[] temp = new byte[this.requests.Length - this.messageLength * nb];
                    for (int i = this.messageLength * nb; i < this.requests.Length; i++)
                    {
                        temp[i - this.messageLength * nb] = (byte)this.requests.GetValue(i);
                    }
                    this.requests = temp;

                    return request;
                }
                else
                {
                    //Si le client est inactif et qu'il n'y a pas assez de données pour faire une requête alors on vide la pile
                    if (!this.Active && this.requests.Length >= this.messageLength)
                    {
                        this.requests = null;
                    }
                }
            }

            throw new Exception("Pas assez de données en mémoire");
        }

        /// <summary>
        /// Parse le flux en entrée et exécute la requête correspondante
        /// </summary>
        /// <param name="stream">Stream.</param>
        private object parser(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    //Lecture de l'entête
                    uint token = reader.ReadUInt32();
                    ushort dataSize = reader.ReadUInt16();
                    ushort idController = reader.ReadUInt16();
                    char[] checksum = reader.ReadChars(32);

                    //Calcul du nombre de blocs à récupérer
                    int nbPart = dataSize / this.messageLength;
                    if (dataSize % this.messageLength > 0)
                    {
                        nbPart++;
                    }

                    //Récupèration des blocs
                    byte[] request;
                    try
                    {
                        request = this.getRequestPart(nbPart);
                    }
                    catch
                    {
                        Logger.log(typeof(ClientHandler), "La requête est parvenue incomplète", Logger.LogType.Error);
                        return null;
                    }

                    //Extraction des données des blocs
                    byte[] message = new byte[dataSize];
                    for (ushort i = 0; i < dataSize; i++)
                    {
                        message[i] = (byte)request.GetValue(i);
                    }

                    //Vérification de l'intégrité des données
                    if (Checksum.verify(message, new string(checksum)))
                    {
                        Stream dataStream = new MemoryStream(message);

                        Response response;
                        switch (idController)
                        {
                            case 1:
                                response = ControllerFactory.getUserController().parser(dataStream);
                                break;
                            default:
                                Logger.log(typeof(ClientHandler), "Le controlleur n'existe pas " + idController, Logger.LogType.Error);
                                //Todo reponse erreur
                                break;
                        }
                    }
                    else
                    {
                        Logger.log(typeof(ClientHandler), "Les données sont érronées", Logger.LogType.Error);
                        //Todo reponse erreur
                    }
                }
                catch (Exception e)
                {
                    Logger.log(typeof(ClientHandler), e.Message, Logger.LogType.Error);
                }
            }
            return new object();
        }

        /// <summary>
        /// Détermine si le client à terminé son exécution
        /// </summary>
        /// <returns></returns>
        public bool isActive()
        {
            return this.Active || this.requests != null;
        }
    }
}