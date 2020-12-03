using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace ServerLibrary
{
    class Communicator
    {
        private DateTime now = DateTime.Now;
        private string interlocutor = "";
        private string host = "";
        public string interlocutorAnswere = "";
        public bool connected = true;
        public NetworkStream c1;

        public string Interlocutor
        {
            get => interlocutor;
            set => interlocutor = value;
        }
        public string Host
        {
            get => host;
            set => host = value;
        }

        /// <summary>
        /// Użytkownik wybiera rozmówcę.
        /// </summary>
        /// <param name="loggedPlayers">Zalogowani gracze.</param>
        /// <param name="busy">Gracze aktualnie zajęci</param>
        /// <param name="s">Strumień użytkownika.</param>
        /// <param name="player">Nazwa użytkownika chcącego rozpocząć rozmowę.</param>
        /// <returns>Nazwa wybranego rozmówcy.</returns>
        private string GetInterlocutor(List<User> loggedPlayers, List<User> busy, NetworkStream s, string player)
        {
            List<User> users = new List<User>();
            bool b;

            for(int i=0; i<loggedPlayers.Count; i++)
            {
                b = false;
                for(int j=0; j<busy.Count; j++)
                {
                    if (loggedPlayers[i]==busy[j])
                    {
                        b = true;
                        break;
                    }
                }
                if (b == false && loggedPlayers[i].Login != player) users.Add(loggedPlayers[i]);
            }

            if (users.Count == 0) return null;

            //MessageTransmission.SendMessage(s, "Players that you can communicate with:" + Environment.NewLine);
            string logins = "";
            for (int i=0; i<users.Count; i++)
            {
                logins += users[i].Login + " ";
                //MessageTransmission.SendMessage(s, (i+1).ToString() + ". " + users[i].Login + "." + Environment.NewLine);
            }
            MessageTransmission.SendMessage(s, logins);
            //MessageTransmission.SendMessage(s, Environment.NewLine);
            //MessageTransmission.SendMessage(s, "Choose [type '0' to resign]:" + Environment.NewLine);

            int u = Convert.ToInt32(MessageTransmission.GetMessage(s));
            if (u == 0) return "0";
            else return users[u - 1].Login;
        }
        /// <summary>
        /// Zwraca indeks strumienia klienta.
        /// </summary>
        /// <param name="client">Klient</param>
        /// <param name="loggedPlayers">Zalogowani użytkownicy</param>
        /// <param name="clients">Strumienie klientów.</param>
        /// <returns>Indeks strumienia klienta</returns>
        private int GetClientIndex(string client, List<User> loggedPlayers, List<NetworkStream> clients)
        {
            int clientIndex = -1;

            for (int i = 0; i < loggedPlayers.Count; i++)
            {
                if (loggedPlayers[i].Login == client)
                {
                    for (int j = 0; j < clients.Count; j++)
                    {
                        if (loggedPlayers[i].Hash == clients[j].GetHashCode())
                        {
                            return j;
                        }
                    }
                }
            }
            return clientIndex;
        }
        /// <summary>
        /// Funkcja odpowiedzialna za rozgrywke kamien, papier nozyce
        /// Obaj grajacy uzytkownicy musza byc zalogowani i wybrac opcje play game
        /// </summary>
        /// <param name="rywal1"></param>
        /// <param name="rywal2"></param>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public void Run(Sqlite sql, string player, List<NetworkStream> clients, List<User> loggedPlayers, List<User> busy)
        {
            host = player;
            c1 = clients[GetClientIndex(player, loggedPlayers, clients)];
            Interlocutor = GetInterlocutor(loggedPlayers, busy, c1, player);

            if (Interlocutor != null && Interlocutor != "0")
            {
                NetworkStream c2 = clients[GetClientIndex(Interlocutor, loggedPlayers, clients)];
                //MessageTransmission.SendMessage(c1, Environment.NewLine);
                //MessageTransmission.SendMessage(c1, "Waiting for " + Interlocutor + "..." + Environment.NewLine);

                MessageTransmission.SendMessage(c2, Environment.NewLine + "Do you want to SPEAK with: " + player + "? [y/n]" + Environment.NewLine);

                int s = 0;

                while (s < 100)
                {
                    Thread.Sleep(100);
                    s++;
                    if (interlocutorAnswere != "") break;
                }

                if (interlocutorAnswere == "y" && s < 100)
                {
                    sql.AddConversationToDB(player, Interlocutor);

                    //MessageTransmission.SendMessage(c1, "The conversation began:" + Environment.NewLine);
                    //MessageTransmission.SendMessage(c2, "The conversation began:" + Environment.NewLine);
                    MessageTransmission.SendMessage(c1, "BEGAN");
                    MessageTransmission.SendMessage(c2, "BEGAN");

                    string m="";

                    while (connected)
                    {
                        if (connected) m = MessageTransmission.GetMessage(c1);
                        if (!connected)
                        {
                            MessageTransmission.SendMessage(c1, "The interlocutor ended the conversation." + Environment.NewLine);
                            break;
                        }
                        if (m == "exit")
                        {
                            MessageTransmission.SendMessage(c1, "You have finished the conversation." + Environment.NewLine);
                            connected = false;
                            break;
                        }
                        if (m != "" && connected)
                        {
                            MessageTransmission.SendMessage(c2, m + " [" + player + "]" + " [" + now.ToString("yyyy-MM-dd hh:mm") + "]" + Environment.NewLine);
                            sql.AddMessageToDB(player, Interlocutor, m, now.ToString("yyyy-MM-dd hh:mm"));
                        }
                    }
                    MessageTransmission.SendMessage(c1, Environment.NewLine);
                }
                else
                {
                    this.Interlocutor = "";
                    //MessageTransmission.SendMessage(c1, "The interlocutor quits the conversation. Try to connect again." + Environment.NewLine);
                    MessageTransmission.SendMessage(c1, "quits");
                    //MessageTransmission.SendMessage(c1, Environment.NewLine);
                }
            }
            else if (Interlocutor == null)
            {
                //MessageTransmission.SendMessage(c1, "There are no other players at the moment." + Environment.NewLine);
                MessageTransmission.SendMessage(c1, "noother");
                //MessageTransmission.SendMessage(c1, Environment.NewLine);
            }
            else if (Interlocutor == "0")
            {
                //MessageTransmission.SendMessage(c1, Environment.NewLine);
            }
        }
    }
}
