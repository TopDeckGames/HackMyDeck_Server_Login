using System;

namespace LoginServer.Data
{
    public static class AdapterFactory
    {
        private static UserAdapter userAdapter;
        private static ServerAdapter serverAdapter;

        /// <summary>
        /// Retourne l'instance de l'adapter des utilisateurs
        /// </summary>
        /// <returns>L'adapteur des utilisateurs</returns>
        public static UserAdapter getUserAdapter()
        {
            if (AdapterFactory.userAdapter == null)
            {
                AdapterFactory.userAdapter = new UserAdapter();
            }
            return AdapterFactory.userAdapter;
        }

        /// <summary>
        /// Retourne l'instace de l'adapter des servers
        /// </summary>
        /// <returns></returns>
        public static ServerAdapter getServerAdapter()
        {
            if (AdapterFactory.serverAdapter == null)
            {
                AdapterFactory.serverAdapter = new ServerAdapter();
            }
            return AdapterFactory.serverAdapter;
        }
    }
}