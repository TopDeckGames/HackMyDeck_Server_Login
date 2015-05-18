using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoginServer.Model;
using System.Threading;
using LoginServer.Manager;

namespace LoginServer
{
    public class CombatQueue
    {
        private volatile Queue queue = new Queue();
        private Thread matchmakingThread;
        public bool Active { get; set; }

        public CombatQueue()
        {
            this.Active = true;

            this.matchmakingThread = new Thread(new ThreadStart(matchmaking));
            this.matchmakingThread.Start();
        }

        public void stop()
        {
            this.Active = false;
        }

        /// <summary>
        /// Ajoute une entrée dans la queu de recherche de combat
        /// </summary>
        /// <param name="data"></param>
        public void addInQueue(KeyValuePair<User, Object> data)
        {
            this.queue.Enqueue(data);
        }

        /// <summary>
        /// Supprime une entrée de la queu de recherche de combat
        /// </summary>
        /// <param name="user">Utilisateur à supprimer</param>
        public void removeFromQueue(User user)
        {
            Queue temp = new Queue();

            lock(this.queue)
            {
                while (this.queue.Count > 0)
                {
                    KeyValuePair<User, Object> data = (KeyValuePair<User, Object>)this.queue.Dequeue();
                    if (!data.Key.Equals(user))
                    {
                        temp.Enqueue(data);
                    }
                }
                this.queue = temp;
            }
        }

        private void matchmaking()
        {
            while(this.Active)
            {
                lock(this.queue)
                {
                    //Tant qu'il y a plus de 2 personnes dans la queu
                    while (this.queue.Count >= 2)
                    {
                        //On dépile 2 entités
                        KeyValuePair<User, Object> data1 = (KeyValuePair<User, Object>)this.queue.Dequeue();
                        KeyValuePair<User, Object> data2 = (KeyValuePair<User, Object>)this.queue.Dequeue();

                        //On crée un nouveau combat
                        Combat combat = new Combat();
                        combat.User1 = data1.Key;
                        combat.User2 = data2.Key;
                        combat.Deck1 = data1.Value;
                        combat.Deck2 = data2.Value;

                        //On enregistre le combat dans un serveur de combat disponible
                        Model.Server selected = ManagerFactory.getServerManager().registerCombat(combat);

                        //On contacte le serveur de gestion pour rediriger les joueurs
                        Model.Server serv = ManagerFactory.getServerManager().getServer(combat.User1.CurrentServer.Name);
                        ManagerFactory.getServerManager().registerCombat(serv, combat.User1, selected);
                        serv = ManagerFactory.getServerManager().getServer(combat.User2.CurrentServer.Name);
                        ManagerFactory.getServerManager().registerCombat(serv, combat.User2, selected);
                    }
                }
                System.Threading.Thread.Sleep(5000);
            }
        }
    }
}
