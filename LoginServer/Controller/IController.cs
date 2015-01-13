using System;
using System.IO;

namespace LoginServer
{
    public interface IController
    {
        Response parser(Stream stream);
    }
}