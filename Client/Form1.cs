using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        TcpClient Client;
        NetworkStream Stream;

        private string UserLogin;
        private bool connected = false;
        private bool send = false;
        private string messa = "";
        //private Thread tmp1;


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
            textBox8.Visible = false;
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
                UserLogin = textBox1.Text;
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
            textBox8.Visible = false;
            SendMessage(Stream, "6");
            panel6.Visible = false;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            textBox8.Visible = false;

            DialogResult dialogResult = MessageBox.Show("Are you sure?", "Are you sure?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                SendMessage(Stream, "4");
                panel6.Visible = false;
            }
            else if (dialogResult == DialogResult.No)
            {
                
            }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void button13_Click(object sender, EventArgs e)
        {
            SendMessage(Stream, "2");
            textBox8.Visible = true;
            textBox8.Text = GetMessage(Stream);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            panel8.Location = new Point(0,0);
            textBox13.Text = "";
            textBox8.Visible = false;
            panel8.Visible = true;

            Thread tmp1 = new Thread(() => WaitForInvite());
            tmp1.Start();
        }

        private void WaitForInvite()
        {
            string m = "";
            while(true)
            {
                m = "";
                m = GetMessage(Stream);
                if (m.StartsWith("PLAY:"))
                {
                    string[] info = m.Split(':');
                    textBox13.Text = "Do you want to PLAY with: " + info[1] + Environment.NewLine;
                }
                else if(m.StartsWith("You "))
                {
                    textBox13.Text += m;
                    button28.Visible = true;
                    button22.Visible = true;
                    button24.Visible = true;
                    button25.Visible = true;
                }
                else if (m.StartsWith("There ") || m.StartsWith("The oppo"))
                {
                    textBox13.Text += m;
                    button28.Visible = true;
                    button22.Visible = true;
                    button24.Visible = true;
                    button25.Visible = true;
                }
                else if (m.StartsWith("STAT"))
                {
                    m = m.Replace("STAT", "");
                    DialogResult dialogResult = MessageBox.Show(m, UserLogin, MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        SendMessage(Stream, "y");
                        textBox3.Text = "";
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        SendMessage(Stream, "n");
                        button28.Visible = true;
                        button22.Visible = true;
                        button24.Visible = true;
                        button25.Visible = true;
                    }
                }
                else if (m == "q") return;
                else textBox13.Text += m;
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            panel7.Location = new Point(0, 0);
            panel7.Visible = true;
            SendMessage(Stream, "3");
            textBox8.Visible = false;
            string l = GetMessage(Stream);

            if(l!="noother")
            {
                string[] logins = l.Split(' ');

                foreach (string i in logins)
                    comboBox1.Items.Add(i);
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            textBox8.Visible = false;
            label20.Visible = true;
            textBox9.Visible = true;
            button18.Visible = true;
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void button18_Click(object sender, EventArgs e)
        {
            string password = textBox9.Text;

            if (password != "")
            {
                SendMessage(Stream, "5");
                SendMessage(Stream, password);
            }
            else if (password == "")
            {
                label9.Visible = true;
                button7.Visible = true;
                label9.Text = "You did not provide your password!";
                return;
            }

            string m = GetMessage(Stream);

            if (m == "success")
            {
                MessageBox.Show("Password was changed successfully.");
                label20.Visible = false;
                textBox9.Visible = false;
                button18.Visible = false;
            }
            else if (m == "wrong")
            {
                MessageBox.Show("Password should be 5 to 20 characters long. Try again.");
            }
        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button21_Click(object sender, EventArgs e)
        {
            if(connected==false) SendMessage(Stream, "0");
            else SendMessage(Stream, "exit");
            panel7.Visible = false;
            connected = false;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            string choose = comboBox1.Text;
            string index = (comboBox1.SelectedIndex+1).ToString();

            SendMessage(Stream, index);

            if (choose!="")
            {
                connected = true;
                textBox11.Text = "Waiting for " + choose + "..." + Environment.NewLine;

                Thread t = new Thread(() => messa = GetMessage(Stream));
                Thread t2 = new Thread(() => check(choose));
                t.Start();
                t2.Start();
            }
        }

        private void WaitForMessages()
        {
            string m = "";
            while(connected)
            {
                m = GetMessage(Stream);
                textBox11.Text += m + Environment.NewLine;
            }
        }
        private void SendMessages()
        {
            while(connected)
            {
                if(send)
                {
                    SendMessage(Stream, textBox10.Text + Environment.NewLine);
                    textBox10.Text = "";
                    send = false;
                }
            }
        }

        private void check(string choose)
        {
            while (messa == "") ;

            if (messa == "BEGAN")
            {
                textBox11.Text += "Waiting for " + choose + "..." + Environment.NewLine;
                textBox11.Text += "The conversation began..." + Environment.NewLine;

                Thread t3 = new Thread(() => WaitForMessages());
                Thread t4 = new Thread(() => SendMessages());
                t3.Start();
                t4.Start();
            }
            else if (messa == "quits")
            {
                textBox11.Text += "The interlocutor quits the conversation. Try to connect again." + Environment.NewLine;
            }
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {

        }

        private void button20_Click(object sender, EventArgs e)
        {
            send = true;
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void button22_Click(object sender, EventArgs e)
        {
            textBox13.Text = "";
            button28.Visible = false;
            button22.Visible = false;
            button24.Visible = false;
            button25.Visible = false;

            SendMessage(Stream, "1");
        }


        private void textBox13_TextChanged(object sender, EventArgs e)
        {

        }

        private void button24_Click(object sender, EventArgs e)
        {
            SendMessage(Stream, "y");
        }

        private void button25_Click(object sender, EventArgs e)
        {
            textBox13.Text = "";
            SendMessage(Stream, "n");
        }

        private void button23_Click(object sender, EventArgs e)
        {
            SendMessage(Stream, "r");
        }

        private void button26_Click(object sender, EventArgs e)
        {
            SendMessage(Stream, "s");
        }

        private void button27_Click(object sender, EventArgs e)
        {
            SendMessage(Stream, "p");
        }

        private void button28_Click(object sender, EventArgs e)
        {
            panel8.Visible = false;
            SendMessage(Stream, "q");
        }
    }
}
