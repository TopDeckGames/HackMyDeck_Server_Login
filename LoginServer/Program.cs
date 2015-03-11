using System;

namespace LoginServer
{
    class MainClass
    {
        private static Server server;
        private static Monitor monitor;

        public static void Main(string[] args)
        {
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