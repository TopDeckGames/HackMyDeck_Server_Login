using System;

namespace LoginServer.Data
{
    public static class AdapterManager
    {
        private static UserAdapter userAdapter;

        /// <summary>
        /// Retourne l'instance de l'adapter des utilisateurs
        /// </summary>
        /// <returns>L'adapteur des utilisateurs</returns>
        public static UserAdapter getUserAdapter()
        {
            if (AdapterManager.userAdapter == null)
            {
                AdapterManager.userAdapter = new UserAdapter();
            }
            return AdapterManager.userAdapter;
        }
    }
}