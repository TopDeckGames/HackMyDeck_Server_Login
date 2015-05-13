using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoginServer.Model
{
    public class Combat
    {
        public User User1 { get; set; }
        public User User2 { get; set; }
        public Object Deck1 { get; set; }
        public Object Deck2 { get; set; }
    }
}