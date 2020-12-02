using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream stream;
     
        public Form1()
        {
            InitializeComponent();
           
            
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            client = new TcpClient("127.0.0.1", 2048);
            stream = client.GetStream();
            textBox1.Text = "Connected to the server..";
            getMessage(stream);
        }
        void SendMessage(NetworkStream stream, string message)
        {
           
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(message);
            stream.Write(bytesToSend, 0, bytesToSend.Length);

        }
        private void button2_Click(object sender, EventArgs e)
        {

            SendMessage(stream, textBox2.Text);
            getMessage(stream);

        }

        void getMessage(NetworkStream stream)
        {

                String response = "";
                Byte[] dataResponse = new Byte[256];
                Int32 bytes = stream.Read(dataResponse, 0, dataResponse.Length);
                response = System.Text.Encoding.ASCII.GetString(dataResponse, 0, bytes);
                textBox1.Text = response;
        }

        
        private void button4_Click(object sender, EventArgs e)
        {
            SendMessage(stream, textBox2.Text);
            getMessage(stream);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            SendMessage(stream, "3");
            getMessage(stream);
        }
    }

        
}
