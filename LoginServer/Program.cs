using System;
using LoginServer.Data;
using LoginServer.Manager;
using LoginServer.Model;

namespace LoginServer
{
    class MainClass
    {
        private static Server server;
        private static Monitor monitor;

        public static void Main(string[] args)
        {
            Logger.log(typeof(MainClass), "Chargement de la configuration", Logger.LogType.Info);
            try
            {
                foreach (Model.Server s in AdapterFactory.getServerAdapter().getAllServers())
                {
                    ManagerFactory.getServerManager().addServer(s);
                }
            }
            catch (Exception e)
            {
                Logger.log(typeof(MainClass), "Impossible de charger la configuration " + e.Message, Logger.LogType.Fatal);
                return;
            }

            Logger.log(typeof(MainClass), "Test de connexion aux serveurs distants", Logger.LogType.Info);
            ManagerFactory.getServerManager().checkServers();

            server = new Server();
            monitor = new Monitor();

            while (true)
            {
                Console.Write("> ");
                string cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "stop":
                    case "exit":
                        server.stop();
                        monitor.stop();
                        Environment.Exit(0);
                        break;
                    case "restart":
                        server.stop();
                        server = new Server();
                        break;
                    case "info":
                        server.info();
                        break;
                    case "servers info":
                        Console.WriteLine(ManagerFactory.getServerManager().ToString());
                        break;
                    case "servers check":
                        ManagerFactory.getServerManager().checkServers();
                        break;
                    case "":
                        break;
                    default:
                        Logger.log(typeof(MainClass), "Commande inconnue : " + cmd, Logger.LogType.Warn);
                        break;
                }
            }
        }
    }
}