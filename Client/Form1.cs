using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        TcpClient Client;
        NetworkStream Stream;


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


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel2.Location = new Point(0, 0);
            panel2.Visible = true;
            button6.Visible = false;
            label6.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel3.Location = new Point(0, 0);
            panel3.Visible = true;
            button7.Visible = false;
            label9.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel5.Location = new Point(0, 0);
            panel5.Visible = true;

            SendMessage(Stream, "3");
            string m = GetMessage(Stream);

            textBox7.Text = m;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Client = new TcpClient("127.0.0.1", 2048);
                Stream = Client.GetStream();
                panel1.Visible = true;
                label1.Visible = false;
            }
            catch(Exception x)
            {
                label1.Visible = true;
                label1.Text = "Unable to connect to server. Try again.";
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text;
            string password = textBox2.Text;

            if (login!="" && password!="")
            {
                SendMessage(Stream, "1");
                SendMessage(Stream, login + " " + password);
            }
            else if (login == "" || password == "")
            {
                label6.Visible = true;
                button6.Visible = true;
                label6.Text = "You did not provide your password or login!";
                return;
            }

            string m = GetMessage(Stream);

            if (m=="success")
            {
                panel2.Visible = false;
                panel6.Visible = true;
            }
            else if (m == "loggedin")
            {
                label6.Visible = true;
                button6.Visible = true;
                label6.Text = "User already logged in.";
            }
            else if (m == "wrong")
            {
                label6.Visible = true;
                button6.Visible = true;
                label6.Text = "Wrong login or password.";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            label6.Visible = false;
            panel2.Visible = false;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            string login = textBox4.Text;
            string password = textBox3.Text;

            if (login != "" && password != "")
            {
                SendMessage(Stream, "2");
                SendMessage(Stream, login + " " + password);
            }
            else if (login == "" || password == "")
            {
                label9.Visible = true;
                button7.Visible = true;
                label9.Text = "You did not provide your password or login!";
                return;
            }

            string m = GetMessage(Stream);

            if (m == "success")
            {
                label9.Visible = true;
                label9.Text = "Registered in";
                button7.Visible = true;
            }
            else if (m == "userlength")
            {
                label9.Visible = true;
                button7.Visible = true;
                label9.Text = "Invalid username length.";
            }
            else if (m == "passlength")
            {
                label9.Visible = true;
                button7.Visible = true;
                label9.Text = "Invalid password length.";
            }
            else if (m == "exist")
            {
                label9.Visible = true;
                button7.Visible = true;
                label9.Text = "User with that name already exists.";
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            panel3.Visible = false;
        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            panel5.Visible = false;
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel6_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button17_Click(object sender, EventArgs e)
        {
            SendMessage(Stream, "6");
            panel6.Visible = false;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            SendMessage(Stream, "4");
            panel6.Visible = false;
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void button13_Click(object sender, EventArgs e)
        {

        }
    }
}
