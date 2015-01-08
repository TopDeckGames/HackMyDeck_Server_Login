using System;
using System.Configuration;
using System.IO;
using System.Text;

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

        private static volatile bool trace;
        private static volatile string filePath;
        private static Object o = new object();

        /// <summary>
        /// Log un évènement
        /// </summary>
        /// <param name="classe">Classe à l'origine du log</param>
        /// <param name="message">Message à logguer</param>
        /// <param name="type">Type de log</param>
        public static void log(Type classe, string message, LogType type)
        {
            String log = DateTime.Now.ToString("[HH:mm:ss]") + " " + type + " - " + message + " (" + classe.ToString() + ")";

            Console.WriteLine(log);

            lock (Logger.o)
            {
                if (Logger.filePath == null)
                {
                    Logger.trace = bool.Parse(ConfigurationManager.AppSettings["trace"]);
                    Logger.filePath = ConfigurationManager.AppSettings["logs_path"] + "log_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                }

                if (Logger.trace)
                {
                    try
                    {
                        StreamWriter file = new StreamWriter(Logger.filePath, true);
                        file.WriteLine(log);
                        file.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Impossible d'enregistrer les logs : " + e.Message);
                    }
                }
            }
        }
    }
}