using System;
using System.IO;

namespace LoginServer
{
    public interface IManager
    {
        Response parser(BinaryReader reader);
    }
}