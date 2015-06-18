using System;

namespace LoginServer.Model
{
    public class User
    {
        public const int LOGIN_LENGTH = 50;
        public const int PASSWORD_LENGTH = 16;
        public const int FIRSTNAME_LENGTH = 75;
        public const int LASTNAME_LENGTH = 75;
        public const int EMAIL_LENGTH = 60;

        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public int Credit { get; set; }

        public Server CurrentServer { get; set; }

        public User(int id)
        {
            this.Id = id;
        }
    }
}