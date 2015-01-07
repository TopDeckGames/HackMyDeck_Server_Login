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

            if (bool.Parse(ConfigurationManager.AppSettings["trace"]))
            {
                string filePath = ConfigurationManager.AppSettings["logs_path"] + "log_" + DateTime.Now.ToString("yyyyMMdd") + ".log";

                try
                {
                    StreamWriter file = new StreamWriter(filePath, true);
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