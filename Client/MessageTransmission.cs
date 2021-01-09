using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class MessageTransmission
    {
        /// <summary>
        /// Funkcja wysyłająca wiadomość do klienta.
        /// </summary>
        /// <param name="s">Strumień klienta.</param>
        /// <param name="m">Wiadomość.</param>
        public static void SendMessage(NetworkStream s, string m)
        {
            byte[] buffer = new byte[2048];
            buffer = Encoding.ASCII.GetBytes(m);
            s.Write(buffer, 0, buffer.Length);
        }
        /// <summary>
        /// Funkcja pobierająca wiadomość od klienta.
        /// </summary>
        /// <param name="s">Strumień klienta.</param>
        /// <returns>Wiadomość od klienta.</returns>
        public static string GetMessage(NetworkStream s)
        {
            while (true)
            {
                byte[] buffer = new byte[2048];
                s.Read(buffer, 0, buffer.Length);
                string m = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                if (m != "\r\n") return m;
            }
        }
    }
}
