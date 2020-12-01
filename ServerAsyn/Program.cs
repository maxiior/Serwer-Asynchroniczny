using System.Net;
using ServerLibrary;

namespace ServerAsyn
{
    class Program
    {
        public static void Main(string[] args)
        {
            Server<GACServerProtocol> server = new ServerTAP<GACServerProtocol>(IPAddress.Parse("127.0.0.1"), 2048);
            server.Start();
        }
    }
}