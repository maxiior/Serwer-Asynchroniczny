using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServerLibrary;

namespace ServerAsyn
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new ServerAPM(IPAddress.Parse("127.0.0.1"), 2048);
            server.Start();
        }
    }
}