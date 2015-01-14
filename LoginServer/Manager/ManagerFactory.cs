using System;

namespace LoginServer.Manager
{
    public static class ManagerFactory
    {
        private static UserManager userManager;

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
    }
}