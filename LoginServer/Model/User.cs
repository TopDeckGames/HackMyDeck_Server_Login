using System;

namespace LoginServer.Model
{
    public class User
    {
        public const int LOGIN_LENGTH = 50;
        public const int PASSWORD_LENGTH = 32;

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