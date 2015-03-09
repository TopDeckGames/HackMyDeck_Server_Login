using System;
using System.Configuration;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace LoginServer
{
    class Monitor
    {
        private TcpListener tcpListener;
        private int port;
        private Thread listenThread;
        private bool active;

        /// <summary>
        /// Initialise et démarre l'interface de monitoring
        /// </summary>
        public Monitor()
        {
            Logger.log(typeof(Monitor), "Démarrage de l'interface de monitoring", Logger.LogType.Info);
            this.active = true;

            this.port = int.Parse(ConfigurationManager.AppSettings["monitoring_port"]);

            this.tcpListener = new TcpListener(IPAddress.Any, this.port);
            this.listenThread = new Thread(new ThreadStart(listenForClients));
            this.listenThread.Start();
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

                MonitorHandler monitorHandler;
                try
                {
                    monitorHandler = new MonitorHandler(client);
                }
                catch (Exception e)
                {
                    Logger.log(typeof(Server), e.Message, Logger.LogType.Fatal);
                    continue;
                }
            }
        }
    }
}
