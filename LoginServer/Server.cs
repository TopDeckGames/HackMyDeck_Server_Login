using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace LoginServer
{
	public class Server
	{
		private TcpListener tcpListener;
		private Thread listenThread;
		private int port = 3000;

		private List<TcpClient> clients = new List<TcpClient> ();

		/// <summary>
		/// Initialise et démarre le serveur TCP
		/// </summary>
		public Server ()
		{
			Logger.log(typeof(Server), "Démarrage du serveur", Logger.LogType.Info);

			this.tcpListener = new TcpListener (IPAddress.Any, port);
			this.listenThread = new Thread (new ThreadStart (ListenForClients));
			this.listenThread.Start();
		}

		/// <summary>
		/// Attend les connexions et pour chacune lance un nouveau thread
		/// </summary>
		private void ListenForClients()
		{
			this.tcpListener.Start ();

			while (true) {
				TcpClient client = this.tcpListener.AcceptTcpClient ();
				ClientHandler clientHandler;

				try
				{
					clientHandler = new ClientHandler (client);
				}
				catch(Exception e) {
					Logger.log (typeof(Server), e.Message, Logger.LogType.Fatal);
					continue;
				}

				Thread clientThread = new Thread (new ThreadStart (clientHandler.handle));
				clientThread.Start();
			}
		}
	}
}