using System;

namespace LoginServer
{
	public static class Logger
	{
		public enum LogType
		{
			Info,
			Error,
			Debug,
			Fatal,
			Warn
		}

		public static void log(Type classe, string message, LogType type)
		{
			Console.WriteLine (DateTime.Now.ToString("[HH:mm:ss]") + " " + type + " - " + message + " (" + classe.ToString() + ")");
		}
	}
}