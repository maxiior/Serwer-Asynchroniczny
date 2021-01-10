using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace ServerLibrary
{
    class Communicator
    {
        private Sqlite sql;
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
        private string GetInterlocutor(List<User> loggedPlayers, List<User> busy, NetworkStream s, string player, List<User> inDuo, List<User> inSolo)
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
                if(b == false && loggedPlayers[i].Login != player && inDuo.Contains(loggedPlayers[i]) == false && inSolo.Contains(loggedPlayers[i]) == false) users.Add(loggedPlayers[i]);
            }

            if(users.Count == 0) return null;

            int u = Convert.ToInt32(MessageTransmission.GetMessage(s));
            if(u == 0) return "0";
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
        public void Run(Sqlite sql, string player, List<NetworkStream> clients, List<User> loggedPlayers, List<User> busy, List<User> inDuo, List<User> inSolo)
        {
            host = player;
            c1 = clients[GetClientIndex(player, loggedPlayers, clients)];
            Interlocutor = GetInterlocutor(loggedPlayers, busy, c1, player, inDuo, inSolo);

            if (Interlocutor != null && Interlocutor != "0")
            {
                NetworkStream c2 = clients[GetClientIndex(Interlocutor, loggedPlayers, clients)];
                MessageTransmission.SendMessage(c2, "TALKKDo you want to TALK with: " + player + "? [YES/NO]" + Environment.NewLine);

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

                    MessageTransmission.SendMessage(c1, "BEGAN");
                    MessageTransmission.SendMessage(c2, "BEGAN");

                    string history = sql.ReadMessagesHistoryDB(player, Interlocutor);
                    string tmp = "";
                    bool done = false;
                    for (int i = 0; i < history.Length; i++)
                    {
                        tmp = "";
                        for (int j = 0; j < 100; j++)
                        {
                            tmp += history[i * 100 + j];
                            if (1 + j + 100 * i == history.Length)
                            {
                                done = true;
                                break;
                            }
                        }
                        MessageTransmission.SendMessage(c1, tmp);
                        MessageTransmission.SendMessage(c2, tmp);
                        if (done) break;
                    }

                    string m = "";

                    while (connected)
                    {
                        if (connected) m = MessageTransmission.GetMessage(c1);
                        if (!connected)
                        {
                            MessageTransmission.SendMessage(c1, "quits");
                            break;
                        }
                        if (m == "exit")
                        {
                            MessageTransmission.SendMessage(c1, "finish");
                            MessageTransmission.SendMessage(c2, "quits");
                            connected = false;
                            break;
                        }
                        if (m != "" && connected)
                        {
                            MessageTransmission.SendMessage(c2, m + " [" + player + "]" + " [" + now.ToString("yyyy-MM-dd hh:mm") + "]" + Environment.NewLine);
                            sql.AddMessageToDB(player, Interlocutor, m, now.ToString("yyyy-MM-dd hh:mm"));
                        }
                    }
                }
                else
                {
                    this.Interlocutor = "";
                    MessageTransmission.SendMessage(c1, "quits");
                }
            }
            else if (Interlocutor == null) MessageTransmission.SendMessage(c1, "noother");
            else if (Interlocutor == "0") ;
        }
    }
}
