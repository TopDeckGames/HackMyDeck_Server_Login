using System;

namespace LoginServer.Data
{
    public static class AdapterFactory
    {
        private static UserAdapter userAdapter;

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
    }
}