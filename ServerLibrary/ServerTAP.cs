using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerLibrary
{
    public class ServerTAP<T> : Server<T> where T : CommunicationProtocol, new()
    {
        public delegate void TransmissionDataDelegate(NetworkStream Stream);
        private CommunicationProtocol protocol = new T();

        public ServerTAP(IPAddress IP, int port) : base(IP, port)
        {

        }
        /// <summary>
        /// Akceptuje połączenie z klientem.
        /// </summary>
        protected override void AcceptClient()
        {
            while (true)
            {
                TcpClient client = TcpListener.AcceptTcpClient();
                Stream = client.GetStream();

                Task.Run(() => BeginDataTransmission(Stream, client));
            }
        }
        /// <summary>
        /// Menu wyboru opcji użytkownika.
        /// </summary>
        /// <param name="Stream">Strumień użytkownika.</param>
        protected override void BeginDataTransmission(NetworkStream Stream, TcpClient client)
        {
            protocol.Run(Stream, client);
            protocol.Close(client);
            Stream.Close();
            client.Close();
        }

        /// <summary>
        /// Rozpoczyna działanie serwera.
        /// </summary>
        public override void Start()
        {
            StartListening();
            AcceptClient();
        }
    }
}
