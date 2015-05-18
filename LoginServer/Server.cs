using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using LoginServer.Handlers;

namespace LoginServer
{
    public class Server
    {
        private volatile TcpListener tcpListener;
        private Thread listenThread, cleanupThread;
        private volatile int port;
        private volatile List<ClientHandler> handlers = new List<ClientHandler>();
        private volatile bool active;

        /// <summary>
        /// Initialise et démarre le serveur TCP
        /// </summary>
        public Server()
        {
            Logger.log(typeof(Server), "Démarrage du serveur", Logger.LogType.Info);

            this.active = true;

            port = int.Parse(ConfigurationManager.AppSettings["port"]);

            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.listenThread = new Thread(new ThreadStart(listenForClients));
            this.listenThread.Start();

            this.cleanupThread = new Thread(new ThreadStart(cleanUp));
            this.cleanupThread.Start();
        }

        /// <summary>
        /// Attend les connexions et pour chacune lance un nouveau thread
        /// </summary>
        private void listenForClients()
        {
            this.tcpListener.Start();

            while (this.active)
            {
                TcpClient client;

                try
                {
                    client = this.tcpListener.AcceptTcpClient();
                }
                catch
                {
                    continue;
                }
                ClientHandler clientHandler;

                lock (this.handlers)
                {
                    try
                    {
                        clientHandler = new ClientHandler(client);
                        this.handlers.Add(clientHandler);
                    }
                    catch (Exception e)
                    {
                        Logger.log(typeof(Server), e.Message, Logger.LogType.Fatal);
                        continue;
                    }

                    Thread clientThread = new Thread(new ThreadStart(clientHandler.handle));
                    clientThread.Start();
                }
            }
        }

        /// <summary>
        /// Enlève les handlers inactifs de la mémoire
        /// </summary>
        private void cleanUp()
        {
            while (this.active)
            {
                lock (this.handlers)
                {
                    List<ClientHandler> temp = new List<ClientHandler>();
                    foreach (ClientHandler client in this.handlers)
                    {
                        if (client.isActive())
                        {
                            temp.Add(client);
                        }
                    }
                    this.handlers = temp;
                }

                System.Threading.Thread.Sleep(300000);
            }
        }

        /// <summary>
        /// Arrête le serveur et stop tous les threads créés
        /// </summary>
        public void stop()
        {
            Logger.log(typeof(Server), "Arrêt du serveur", Logger.LogType.Info);

            this.active = false;

            lock (this.handlers)
            {
                //Arrêt des threads en cours
                foreach (ClientHandler client in this.handlers)
                {
                    client.Active = false;
                }
            }

            //Fermeture du socket
            this.tcpListener.Stop();
        }

        /// <summary>
        /// Affiche les informations du serveur
        /// </summary>
        public void info()
        {
            Logger.log(typeof(Server), this.handlers.Count + " clients connectés", Logger.LogType.Info);
        }

        /// <summary>
        /// Calcule le nombre de clients connectés
        /// </summary>
        /// <returns>Nb de clients
        public int getNbPlayers()
        {
            return this.handlers.Count;
        }
    }
}