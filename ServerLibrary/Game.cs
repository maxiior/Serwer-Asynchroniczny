using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace ServerLibrary
{
    public class Game
    {
        public List<string> PlayerChoice = new List<string>();
        public List<string> OpponentChoice = new List<string>();

        private int PlayerPoints = 0;
        private int OpponentPoints = 0;

        public string opponentAnswere = "";
        private string opponent = "";
        private string host = "";

        public bool connected = true;

        public string Opponent
        {
            get => opponent;
            set => opponent = value;
        }
        public string Host
        {
            get => host;
            set => host = value;
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
        public void Run(Sqlite sql, string player, List<NetworkStream> clients, List<User> loggedPlayers, List<User> alreadyPlay)
        {
            Host = player;
            NetworkStream c1 = clients[GetClientIndex(player, loggedPlayers, clients)];

            Opponent = sql.SelectOpponent(player, alreadyPlay);

            if(Opponent!=null)
            {
                //MessageTransmission.SendMessage(c1, Environment.NewLine);
                MessageTransmission.SendMessage(c1, "Your opponent is: " + Opponent + Environment.NewLine);
                MessageTransmission.SendMessage(c1, "Waiting for opponent..." + Environment.NewLine);

                NetworkStream c2 = clients[GetClientIndex(Opponent, loggedPlayers, clients)];
                MessageTransmission.SendMessage(c2, "PLAY:" + player);
                //MessageTransmission.SendMessage(c2, Environment.NewLine + "Do you want to PLAY with: " + player + "? [y/n]" + Environment.NewLine);

                int s = 0;

                while (s < 100)
                {
                    Thread.Sleep(100);
                    s++;
                    if (s == 100 || opponentAnswere != "") break;
                }

                if (opponentAnswere == "y" && s < 100)
                {
                    Choose(c1);
                    while (this.OpponentChoice.Count < 1 && connected) ;
                    if(!connected)
                    {
                        Close(c1);
                        return;
                    }

                    if (Compare(PlayerChoice[0], OpponentChoice[0]) == 1)
                    {
                        PlayerPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 1: " + player + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 1: " + player + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    else if (Compare(PlayerChoice[0], OpponentChoice[0]) == 2)
                    {
                        OpponentPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 1: " + Opponent + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 1: " + Opponent + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    else
                    {
                        OpponentPoints++;
                        PlayerPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 1: Draw! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 1: Draw! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    sql.AddMove(player, PlayerChoice[0]);
                    sql.AddMove(Opponent, OpponentChoice[0]);

                    MessageTransmission.SendMessage(c1, Environment.NewLine);

                    Choose(c1);
                    while (this.OpponentChoice.Count < 2 && connected) ;
                    if (!connected)
                    {
                        Close(c1);
                        return;
                    }

                    if (Compare(PlayerChoice[1], OpponentChoice[1]) == 1)
                    {
                        PlayerPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 2: " + player + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 2: " + player + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    else if (Compare(PlayerChoice[1], OpponentChoice[1]) == 2)
                    {
                        OpponentPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 2: " + Opponent + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 2: " + Opponent + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    else
                    {
                        OpponentPoints++;
                        PlayerPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 2: Draw! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 2: Draw! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    sql.AddMove(player, PlayerChoice[1]);
                    sql.AddMove(Opponent, OpponentChoice[1]);

                    MessageTransmission.SendMessage(c1, Environment.NewLine);

                    Choose(c1);
                    while (this.OpponentChoice.Count < 3 && connected) ;
                    if (!connected)
                    {
                        Close(c1);
                        return;
                    }

                    if (Compare(PlayerChoice[2], OpponentChoice[2]) == 1)
                    {
                        PlayerPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 3: " + player + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 3: " + player + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    else if (Compare(PlayerChoice[2], OpponentChoice[2]) == 2)
                    {
                        OpponentPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 3: " + Opponent + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 3: " + Opponent + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    else
                    {
                        OpponentPoints++;
                        PlayerPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 3: Draw! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 3: Draw! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    sql.AddMove(player, PlayerChoice[2]);
                    sql.AddMove(Opponent, OpponentChoice[2]);

                    MessageTransmission.SendMessage(c1, Environment.NewLine);

                    Choose(c1);
                    while (this.OpponentChoice.Count < 4 && connected) ;
                    if (!connected)
                    {
                        Close(c1);
                        return;
                    }

                    if (Compare(PlayerChoice[3], OpponentChoice[3]) == 1)
                    {
                        PlayerPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 4: " + player + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 4: " + player + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    else if (Compare(PlayerChoice[3], OpponentChoice[3]) == 2)
                    {
                        OpponentPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 4: " + Opponent + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 4: " + Opponent + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    else
                    {
                        OpponentPoints++;
                        PlayerPoints++;
                        MessageTransmission.SendMessage(c1, "ROUND 4: Draw! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "ROUND 4: Draw! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                    }
                    sql.AddMove(player, PlayerChoice[3]);
                    sql.AddMove(Opponent, OpponentChoice[3]);

                    if (OpponentPoints > PlayerPoints)
                    {
                        sql.AddWin(Opponent);
                        sql.AddLose(player);
                        sql.SetNewRanking(Opponent, player, 1);
                        sql.SetNewRanking(player, Opponent, 0);
                        MessageTransmission.SendMessage(c1, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "You won the match! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c1, "You lost the match! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine + Environment.NewLine);
                    }
                    else if (OpponentPoints < PlayerPoints)
                    {
                        sql.AddWin(player);
                        sql.AddLose(Opponent);
                        sql.SetNewRanking(Opponent, player, 0);
                        sql.SetNewRanking(player, Opponent, 1);
                        MessageTransmission.SendMessage(c1, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "You lost the match! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c1, "You won the match! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine + Environment.NewLine);
                    }
                    else
                    {
                        sql.AddDraw(player);
                        sql.AddDraw(Opponent);
                        sql.SetNewRanking(Opponent, player, 0.5);
                        sql.SetNewRanking(player, Opponent, 0.5);
                        MessageTransmission.SendMessage(c1, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "You draw the match! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c1, "You draw the match! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine + Environment.NewLine);
                    }
                }
                else
                {
                    this.Opponent = "";
                    MessageTransmission.SendMessage(c1, "The opponent quits the game. Try to connect again." + Environment.NewLine);
                    MessageTransmission.SendMessage(c1, Environment.NewLine);
                }
            }
            else
            {
                MessageTransmission.SendMessage(c1, "There are no other players at the moment." + Environment.NewLine);
                MessageTransmission.SendMessage(c1, Environment.NewLine);
            }
        }

        /// <summary>
        /// Prosi gracza o wybór.
        /// </summary>
        /// <param name="Stream">Strumień konkretnego gracza.</param>
        private void Choose(NetworkStream Stream)
        {
            MessageTransmission.SendMessage(Stream, "Choose [r]ock, [p]aper, [s]cissors: " + Environment.NewLine);
            string w = MessageTransmission.GetMessage(Stream);
            PlayerChoice.Add(w);
        }
        /// <summary>
        /// Porównuje wybory gracza i zwraca kto wygrał.
        /// </summary>
        /// <param name="s1">Wybór pierwszego gracza.</param>
        /// <param name="s2">Wybór drugiego gracza.</param>
        /// <returns>Wynik</returns>
        private int Compare(string s1, string s2)
        {
            if (s1 == "r" && s2 == "r")
            {
                return 0;
            }
            else if (s1 == "s" && s2 == "s")
            {
                return 0;
            }
            else if (s1 == "p" && s2 == "p")
            {
                return 0;
            }
            else if (s1 == "r" && s2 == "s")
            {
                return 1;
            }
            else if (s1 == "s" && s2 == "r")
            {
                return 2;
            }
            else if (s1 == "r" && s2 == "p")
            {
                return 2;
            }
            else if (s1 == "p" && s2 == "r")
            {
                return 1;
            }
            else if (s1 == "s" && s2 == "p")
            {
                return 1;
            }
            else if (s1 == "p" && s2 == "s")
            {
                return 2;
            }
            else return 0;
        }
        private void Close(NetworkStream c1)
        {
            MessageTransmission.SendMessage(c1, Environment.NewLine);
            MessageTransmission.SendMessage(c1, "The opponent quits the game. Try to connect again." + Environment.NewLine);
            MessageTransmission.SendMessage(c1, Environment.NewLine);
        }
    }
}
