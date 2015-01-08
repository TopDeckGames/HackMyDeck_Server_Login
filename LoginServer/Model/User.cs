using System;

namespace LoginServer.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public User(int id, string login, string password)
        {
            this.Id = id;
            this.Login = login;
            this.Password = password;
        }
    }
}