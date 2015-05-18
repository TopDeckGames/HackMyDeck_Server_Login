using System;
using LoginServer.Data;
using LoginServer.Manager;
using LoginServer.Model;

namespace LoginServer
{
    class MainClass
    {
        public static Server Server { get; set; }
        public static CombatQueue CombatQueue { get; set; }
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

            Server = new Server();
            monitor = new Monitor();
            CombatQueue = new CombatQueue();

            while (true)
            {
                Console.Write("> ");
                string cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "stop":
                    case "exit":
                        Server.stop();
                        monitor.stop();
                        CombatQueue.stop();
                        Environment.Exit(0);
                        break;
                    case "restart":
                        Server.stop();
                        CombatQueue.stop();
                        Server = new Server();
                        CombatQueue = new CombatQueue();
                        break;
                    case "info":
                        Server.info();
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