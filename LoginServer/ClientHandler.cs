using System;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.IO;

namespace LoginServer
{
	public class ClientHandler
	{
		private TcpClient tcpClient;
		private int messageLength = 4096;

		/// <summary>
		/// Initialise le ClientHandler
		/// </summary>
		/// <param name="client">Client TCP</param>
		public ClientHandler (object client)
		{
			this.tcpClient = (TcpClient)client;

			if (this.tcpClient.Connected) {
				Logger.log (typeof(ClientHandler), "Client connecté", Logger.LogType.Info);
			} else {
				throw new Exception ("Client non connecté");
			}
		}

		/// <summary>
		/// Ecoute les requêtes du client
		/// </summary>
		public void handle()
		{
			if (this.tcpClient.Connected) {
				NetworkStream clientStream = this.tcpClient.GetStream ();
				byte[] message = new byte[messageLength];
				int bytesRead;

				while (true) {
					bytesRead = 0;

					try
					{
						bytesRead = clientStream.Read(message, 0, messageLength);
					} 
					catch (Exception e) {
						Logger.log(typeof(ClientHandler), "Erreur lors de l'execution du socket : " + e.Message, Logger.LogType.Error);
						break;
					}

					if (bytesRead == 0) {
						Logger.log(typeof(ClientHandler), "Connexion interrompue", Logger.LogType.Info);
						break;
					}
						
					Stream stream = new MemoryStream (message);
					using (BinaryReader reader = new BinaryReader (stream)) {
						var i = reader.ReadInt32();
						var j = reader.ReadUInt32 ();
					}
				}

				clientStream.Close ();
			}

			this.tcpClient.Close ();
		}
	}
}