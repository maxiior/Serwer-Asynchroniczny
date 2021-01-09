using System;
using System.Net;
using System.Net.Sockets;

namespace ServerLibrary
{
    public abstract class Server<T> where T : CommunicationProtocol, new()
    {
        private IPAddress iPAddress;
        private int port;
        private int buffer_size = 1024;
        private bool running;

        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private NetworkStream stream;

        /// <summary>
        /// Zmienna przechowująca adres IP serwera. Nie można jej zmienić w trakcie działania serwera.
        /// </summary>
        public IPAddress IPAddress
        {
            get => iPAddress; 
            set
            {
                if (!running) iPAddress = value;
                else throw new Exception("ERR: nie można zmienić adresu IP, gdy serwer jest aktywny.");
            }
        }
        /// <summary>
        /// Zmienna przechowująca port serwera. Nie można jej zmienić w trakcie działania serwera.
        /// </summary>
        public int Port
        {
            get => port; 
            set
            {
                int tmp = port;

                if (!running) port = value;
                else throw new Exception("ERR: nie można zmienić portu, gdy serwer jest aktywny.");

                if (!checkPort())
                {
                    port = tmp;
                    throw new Exception("ERR: błąd wartości portu.");
                }
            }
        }
        /// <summary>
        /// Zmienna przechowująca rozmiar buffera. Nie można jej zmienić w trakcie działania serwera. 
        /// </summary>
        public int Buffer_size
        {
            get => buffer_size; 
            set
            {
                if (value < 0 || value > 1024 * 1024 * 64) throw new Exception("ERR: błędny rozmiar pakietu.");

                if (!running) buffer_size = value;
                else throw new Exception("ERR: nie można zmienić rozmiaru pakietu, gdy serwer jest aktywny.");
            }
        }

        protected TcpListener TcpListener { get => tcpListener; set => tcpListener = value; }
        protected TcpClient TcpClient { get => tcpClient; set => tcpClient = value; }
        protected NetworkStream Stream { get => stream; set => stream = value; }
        /// <summary>
        /// Domyślny konstruktor serwera.
        /// </summary>
        /// <param name="IP">Adres IP serwera.</param>
        /// <param name="port">Port serwera.</param>
        public Server(IPAddress IP, int port)
        {
            running = false;
            IPAddress = IP;
            Port = port;

            if (!checkPort())
            {
                Port = 8000;
                throw new Exception("ERR: błędna wartość portu. Ustawiam port na 8000.");
            }
        }
        /// <summary>
        /// Sprawdza, czy port jest poprawny.
        /// </summary>
        /// <returns>Wartość inforumująca, czy port jest poprawny.</returns>
        protected bool checkPort()
        {
            if (port < 1024 || port > 49151) return false;
            return true;
        }
        /// <summary>
        /// Nasłuchiwanie.
        /// </summary>
        protected void StartListening()
        {
            TcpListener = new TcpListener(IPAddress, Port);
            TcpListener.Start();
        }
        /// <summary>
        /// Połączenie klienta.
        /// </summary>
        protected abstract void AcceptClient();
        /// <summary>
        /// System logowania i przesyłu danych.
        /// </summary>
        protected abstract void BeginDataTransmission(NetworkStream stream, TcpClient client);
        /// <summary>
        /// Uruchomienie serwera.
        /// </summary>
        public abstract void Start();
    }
}
