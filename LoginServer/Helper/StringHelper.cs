using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoginServer.Helper
{
    public static class StringHelper
    {
        private const char SEPARATOR = '*';

        /// <summary>
        /// Enlève les charactères de remplissage d'une chaine
        /// </summary>
        /// <param name="str">Chaine réelle</param>
        /// <returns></returns>
        public static string getTrueString(string str)
        {
            return str.Split(StringHelper.SEPARATOR)[0];
        }
    }
}
