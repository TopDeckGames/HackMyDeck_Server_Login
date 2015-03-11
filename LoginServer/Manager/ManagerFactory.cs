using System;

namespace LoginServer.Manager
{
    public static class ManagerFactory
    {
        private static UserManager userManager;
        private static ServerManager serverManager;

        /// <summary>
        /// Récupère le manager des utilisateurs
        /// </summary>
        /// <returns></returns>
        public static UserManager getUserManager()
        {
            if (ManagerFactory.userManager == null)
            {
                ManagerFactory.userManager = new UserManager();
            }
            return ManagerFactory.userManager;
        }

        /// <summary>
        /// Recupère le manager des serveurs
        /// </summary>
        /// <returns></returns>
        public static ServerManager getServerManager()
        {
            if (ManagerFactory.serverManager == null)
            {
                ManagerFactory.serverManager = new ServerManager();
            }
            return ManagerFactory.serverManager;
        }
    }
}