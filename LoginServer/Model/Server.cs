using System;
using System.Net;

namespace LoginServer.Model
{
    public class Server
    {
        public enum ServerType { Gestion = 1, Combat = 2 };

        public string Name { get; set; }
        public IPAddress Address { get; set; }
        public int Port { get; set; }
        public int PortMonitoring { get; set; }
        public ServerType Type { get; set; }
        public int MaxPlayers { get; set; }
        public int NbPlayers { get; set; }
        public bool Available { get; set; }
    }
}