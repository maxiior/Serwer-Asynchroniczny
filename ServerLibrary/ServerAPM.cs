using System;
using System.Net;
using System.Net.Sockets;

namespace ServerLibrary
{
    public class ServerAPM<T> : Server<T> where T : CommunicationProtocol, new()
    {
        public delegate void TransmissionDataDelegate(NetworkStream Stream, TcpClient client);
        private CommunicationProtocol protocol = new T();

        public ServerAPM(IPAddress IP, int port) : base(IP, port)
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
                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(BeginDataTransmission);
                transmissionDelegate.BeginInvoke(Stream, client, TransmissionCallback, client);
            }
        }
        /// <summary>
        /// Kończy połączenie z klientem.
        /// </summary>
        /// <param name="ar"></param>
        private void TransmissionCallback(IAsyncResult ar)
        {
            TcpClient c = (TcpClient)ar.AsyncState;
            c.Close();
        }
        /// <summary>
        /// Menu wyboru opcji użytkownika.
        /// </summary>
        /// <param name="Stream">Strumień użytkownika.</param>
        protected override void BeginDataTransmission(NetworkStream Stream, TcpClient client)
        {
            protocol.Run(Stream, client);
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
