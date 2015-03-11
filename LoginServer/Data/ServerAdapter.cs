using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Net;
using LoginServer.Model;

namespace LoginServer.Data
{
    public class ServerAdapter
    {
        private const string FILE_PATH = "Servers.xml";

        private XmlDocument document;

        public ServerAdapter()
        {
            this.document = new XmlDocument();
            this.document.Load(ServerAdapter.FILE_PATH);
        }

        /// <summary>
        /// Sauvegarde un nouveau serveur
        /// </summary>
        /// <param name="server">Serveur à ajouter</param>
        public void addServer(Model.Server server)
        {
            XmlElement serv = this.document.CreateElement("Server");
            serv.SetAttribute("Name", server.Name);
            serv.AppendChild(this.document.CreateElement("IpAddress")).InnerText = server.Address.ToString();
            serv.AppendChild(this.document.CreateElement("Port")).InnerText = Convert.ToString(server.Port);
            serv.AppendChild(this.document.CreateElement("PortMonitoring")).InnerText = Convert.ToString(server.PortMonitoring);
            serv.AppendChild(this.document.CreateElement("MaxPlayers")).InnerText = Convert.ToString(server.MaxPlayers);
            serv.AppendChild(this.document.CreateElement("Type")).InnerText = Convert.ToString(server.Type);
            this.document.AppendChild(serv);
            this.document.Save(ServerAdapter.FILE_PATH);
        }

        /// <summary>
        /// Récupère tous les serveurs enregistrés
        /// </summary>
        /// <returns>Liste de serveurs</returns>
        public List<Model.Server> getAllServers()
        {
            List<Model.Server> servers = new List<Model.Server>();

            XmlNodeList nodeList = this.document.SelectNodes("/Servers/Server");

            foreach (XmlNode node in nodeList)
            {
                Model.Server server = new Model.Server();
                server.Name = node.Attributes["Name"].Value;
                server.Address = IPAddress.Parse(node.SelectSingleNode("IpAddress").InnerText);
                server.Port = int.Parse(node.SelectSingleNode("Port").InnerText);
                server.PortMonitoring = int.Parse(node.SelectSingleNode("PortMonitoring").InnerText);
                server.MaxPlayers = int.Parse(node.SelectSingleNode("MaxPlayers").InnerText);
                server.Type = (Model.Server.ServerType)Enum.Parse(typeof(Model.Server.ServerType), node.SelectSingleNode("Type").InnerText);
                servers.Add(server);
            }

            return servers;
        }
    }
}
