using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoginServer.Model
{
    public class Request
    {
        public enum TypeRequest { Check = 1, Register = 2, Response = 3, EnterCombat = 4, LeaveCombat = 5 }

        public TypeRequest Type { get; set; }
        public string Data { get; set; }
        public byte[] ByteData { get; set; }
    }
}