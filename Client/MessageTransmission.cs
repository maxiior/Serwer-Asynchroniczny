using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class MessageTransmission
    {
        public static void SendMessage(NetworkStream s, string m)
        {
            byte[] buffer = new byte[2048];
            buffer = Encoding.ASCII.GetBytes(m);
            s.Write(buffer, 0, buffer.Length);
        }

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
