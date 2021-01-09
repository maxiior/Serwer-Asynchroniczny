using System.Net.Sockets;

namespace ServerLibrary
{
    public abstract class CommunicationProtocol
    {
        public CommunicationProtocol()
        {

        }
        public abstract void Run(NetworkStream Stream, TcpClient client);
        public abstract void Close(TcpClient client);
        public abstract string GetName();
    }
}
