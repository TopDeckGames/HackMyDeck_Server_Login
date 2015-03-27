using System;
using System.Security.Cryptography;
using System.Text;

namespace LoginServer.Helper
{
    public static class Checksum
    {
        /// <summary>
        /// Vérifie si des données avec un hash
        /// </summary>
        /// <param name="data">Données à vérifier</param>
        /// <param name="checksum">Hash</param>
        public static bool verify(byte[] data, string checksum)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] hash = md5Hash.ComputeHash(data);

                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)
                {
                    sBuilder.Append(hash[i].ToString("x2"));
                }

                return checksum.Equals(sBuilder.ToString());
            }
        }

        /// <summary>
        /// Crée la chaine de vérification associée à un ensemble de données
        /// </summary>
        /// <param name="data">Données</param>
        /// <returns>Chaine de vérification</returns>
        public static string create(byte[] data)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] hash = md5Hash.ComputeHash(data);

                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)
                {
                    sBuilder.Append(hash[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }
    }
}