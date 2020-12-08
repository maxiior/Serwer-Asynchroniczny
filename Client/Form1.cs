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

        private DateTime now = DateTime.Now;
        private string UserLogin;
        private bool connected = false;
        private bool send = false;
        private string messa = "";

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

            MessageTransmission.SendMessage(Stream, "3");
            string m = MessageTransmission.GetMessage(Stream);

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
                MessageTransmission.SendMessage(Stream, "1");
                MessageTransmission.SendMessage(Stream, login + " " + password);
            }
            else if (login == "" || password == "")
            {
                label6.Visible = true;
                button6.Visible = true;
                label6.Text = "You did not provide your password or login!";
                return;
            }

            string m = MessageTransmission.GetMessage(Stream);

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
                MessageTransmission.SendMessage(Stream, "2");
                MessageTransmission.SendMessage(Stream, login + " " + password);
            }
            else if (login == "" || password == "")
            {
                label9.Visible = true;
                button7.Visible = true;
                label9.Text = "You did not provide your password or login!";
                return;
            }

            string m = MessageTransmission.GetMessage(Stream);

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
            MessageTransmission.SendMessage(Stream, "6");
            panel6.Visible = false;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            textBox8.Visible = false;

            DialogResult dialogResult = MessageBox.Show("Are you sure?", "Are you sure?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                MessageTransmission.SendMessage(Stream, "4");
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
            MessageTransmission.SendMessage(Stream, "2");
            textBox8.Visible = true;
            textBox8.Text = MessageTransmission.GetMessage(Stream);
        }

        //Game
        private void button12_Click(object sender, EventArgs e)
        {
            MessageTransmission.SendMessage(Stream, "nobussy");
            panel8.Location = new Point(0,0);
            textBox13.Text = "";
            textBox8.Visible = false;
            panel8.Visible = true;

            Thread tmp1 = new Thread(() => WaitForInviteGame());
            tmp1.Start();
        }

        private void WaitForInviteGame()
        {
            string m = "";
            while(true)
            {
                m = "";
                m = MessageTransmission.GetMessage(Stream);
                if (m.StartsWith("PLAY:"))
                {
                    connected = true;
                    string[] info = m.Split(':');
                    textBox13.Text = "Do you want to PLAY with: " + info[1] + "? [YES/NO]" + Environment.NewLine;
                }
                else if (m.StartsWith("You "))
                {
                    textBox13.Text += m;
                    button28.Visible = true;
                    button22.Visible = true;
                    button24.Visible = true;
                    button25.Visible = true;
                    connected = false;
                }
                else if (m.StartsWith("There ") || m.StartsWith("The oppo") || m.StartsWith("Opponent left"))
                {
                    if(m.StartsWith("Opponent left") && connected) textBox13.Text += m;
                    else textBox13.Text += m;
                    button28.Visible = true;
                    button22.Visible = true;
                    button24.Visible = true;
                    button25.Visible = true;
                    connected = false;
                }
                else if (m.StartsWith("STAT"))
                {
                    m = m.Replace("STAT", "");
                    DialogResult dialogResult = MessageBox.Show(m, UserLogin, MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        MessageTransmission.SendMessage(Stream, "y");
                        connected = true;
                        button22.Visible = false;
                        button25.Visible = false;
                        button24.Visible = false;
                        button28.Visible = false;
                        textBox13.Text = "";
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        MessageTransmission.SendMessage(Stream, "n");
                        button28.Visible = true;
                        button22.Visible = true;
                        button24.Visible = true;
                        button25.Visible = true;
                        connected = false;
                    }
                }
                else if (m == "q")
                {
                    connected = false;
                    return;
                }
                else textBox13.Text += m;
            }
        }

        //Communicator
        private void button14_Click(object sender, EventArgs e)
        {
            MessageTransmission.SendMessage(Stream, "nobussy");
            MessageTransmission.SendMessage(Stream, "freeusers");
            panel7.Location = new Point(0, 0);
            textBox11.Text = "";
            textBox8.Visible = false;
            panel7.Visible = true;

            string m = MessageTransmission.GetMessage(Stream);

            comboBox1.Items.Clear();

            if (m != "noother")
            {
                m = m.Replace("LOGINS", "");
                
                string[] logins = m.Split(':');

                for (int i = 0; i < logins.Length; i++)
                {
                    comboBox1.Items.Add(logins[i]);
                }
            }
            else textBox11.Text += "There are no other players at the moment.";

            Thread tmp1 = new Thread(() => WaitForInviteCommunicator());
            tmp1.Start();
        }

        private void WaitForInviteCommunicator()
        {
            string m = "";
            while (true)
            {
                m = "";
                m = MessageTransmission.GetMessage(Stream);

                if (m == "BEGAN")
                {
                    textBox11.Text = "";
                    connected = true;
                    textBox11.Text += "The conversation began..." + Environment.NewLine;

                    Thread t4 = new Thread(() => SendMessages());
                    t4.Start();
                }
                else if (m == "quits" && connected == true)
                {
                    connected = false;
                    button21.Visible = true;
                    button29.Visible = true;
                    button30.Visible = true;
                    button19.Visible = true;
                    button32.Visible = true;
                    comboBox1.Visible = true;
                    MessageTransmission.SendMessage(Stream, "sth");
                    textBox11.AppendText("The interlocutor quits the conversation. Try to connect again." + Environment.NewLine);
                }
                else if (m == "noother")
                {
                    button21.Visible = true;
                    button29.Visible = true;
                    button30.Visible = true;
                    button32.Visible = true;
                    comboBox1.Visible = true;
                    button19.Visible = true;
                    textBox11.Text = "";
                    textBox11.Text += "There are no other players at the moment." + Environment.NewLine;
                }
                else if(m == "finish")
                {
                    button21.Visible = true;
                    button29.Visible = true;
                    button32.Visible = true;
                    button30.Visible = true;
                    comboBox1.Visible = true;
                    button19.Visible = true;
                    textBox10.Text = "";
                    connected = false;
                    textBox11.AppendText("You have finished the conversation." + Environment.NewLine);
                }
                else if(m.StartsWith("TALKK"))
                {
                    connected = true;
                    textBox11.Text = "";
                    textBox11.Text += m.Replace("TALKK","");
                }
                else if (m.StartsWith("LOGINS"))
                {
                    m = m.Replace("LOGINS", "");
                    comboBox1.Items.Clear();
                    if (m!="")
                    {
                        string[] logins = m.Split(':');

                        for (int i = 0; i < logins.Length; i++)
                        {
                            comboBox1.Items.Add(logins[i]);
                        }
                    }
                }
                else if (m == "q") return;
                else if (m != "quits") textBox11.AppendText(m);
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
                MessageTransmission.SendMessage(Stream, "5");
                MessageTransmission.SendMessage(Stream, password);
            }
            else if (password == "")
            {
                label9.Visible = true;
                button7.Visible = true;
                label9.Text = "You did not provide your password!";
                return;
            }

            string m = MessageTransmission.GetMessage(Stream);

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
            MessageTransmission.SendMessage(Stream, "q");

            panel7.Visible = false;
            connected = false;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            connected = true;
            string choose = comboBox1.Text;
            string index = (comboBox1.SelectedIndex+1).ToString();

            if (comboBox1.Items.Count > 0 && index == "0")
                index = "1";

            button21.Visible = false;
            button29.Visible = false;
            button30.Visible = false;
            button19.Visible = false;
            button32.Visible = false;
            comboBox1.Visible = false;

            MessageTransmission.SendMessage(Stream, "3");
            MessageTransmission.SendMessage(Stream, index);

            if (choose!="")
            {
                textBox11.Text = "Waiting for " + choose + "..." + Environment.NewLine;
            }
        }
        
        private void SendMessages()
        {
            send = false;
            while(connected)
            {
                if(send)
                {
                    MessageTransmission.SendMessage(Stream, textBox10.Text);
                    if(textBox10.Text!="exit") textBox11.AppendText(textBox10.Text + " [" +UserLogin + "]" + " [" + now.ToString("yyyy-MM-dd hh:mm") + "]" + Environment.NewLine);
                    else return;
                    textBox10.Text = "";
                    send = false;
                }
            }
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {

        }

        private void button20_Click(object sender, EventArgs e)
        {
            
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void button22_Click(object sender, EventArgs e)
        {
            connected = true;
            textBox13.Text = "";
            button28.Visible = false;
            button22.Visible = false;
            button24.Visible = false;
            button25.Visible = false;

            MessageTransmission.SendMessage(Stream, "1");
        }


        private void textBox13_TextChanged(object sender, EventArgs e)
        {

        }

        private void button24_Click(object sender, EventArgs e)
        {
            if(connected)
            {
                button22.Visible = false;
                button25.Visible = false;
                button24.Visible = false;
                button28.Visible = false;
            }
            MessageTransmission.SendMessage(Stream, "y");
        }

        private void button25_Click(object sender, EventArgs e)
        {
            textBox13.Text = "";
            MessageTransmission.SendMessage(Stream, "n");
        }

        private void button23_Click(object sender, EventArgs e)
        {
            MessageTransmission.SendMessage(Stream, "r");
        }

        private void button26_Click(object sender, EventArgs e)
        {
            MessageTransmission.SendMessage(Stream, "s");
        }

        private void button27_Click(object sender, EventArgs e)
        {
            MessageTransmission.SendMessage(Stream, "p");
        }

        private void button28_Click(object sender, EventArgs e)
        {
            panel8.Visible = false;
            MessageTransmission.SendMessage(Stream, "q");
        }

        private void button30_Click(object sender, EventArgs e)
        {
            MessageTransmission.SendMessage(Stream, "yc");
            if(connected)
            {
                button21.Visible = false;
                button29.Visible = false;
                button32.Visible = false;
                button30.Visible = false;
                button19.Visible = false;
                comboBox1.Visible = false;
            }
        }

        private void button29_Click(object sender, EventArgs e)
        {
            if (connected == true) textBox11.Text = "";
            connected = false;
            MessageTransmission.SendMessage(Stream, "n");
        }

        private void button20_Click_1(object sender, EventArgs e)
        {
            send = true;
        }

        private void button31_Click(object sender, EventArgs e)
        {
            if (connected == true) MessageTransmission.SendMessage(Stream, "exit");
        }

        private void button32_Click(object sender, EventArgs e)
        {
            MessageTransmission.SendMessage(Stream, "freeusers");
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
