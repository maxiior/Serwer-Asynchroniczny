using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace ServerLibrary
{
    public class DuoGame
    {
        public List<string> PlayerChoice = new List<string>();
        public List<string> TeammateChoice = new List<string>();
        public List<string> Opponent1Choice = new List<string>();
        public List<string> Opponent2Choice = new List<string>();

        public string opponent1Answere = "";
        public string opponent2Answere = "";
        public string TeammateAnswere = "";
        public string hostAnswere = "";

        public bool connected = true;
        private int PlayerPoints = 0;
        private int OpponentPoints = 0;

        private string opponent1 = "";
        private string opponent2 = "";
        private string host = "";
        private string teammate = "";

        public string Opponent1
        {
            get => opponent1;
            set => opponent1 = value;
        }
        public string Opponent2
        {
            get => opponent2;
            set => opponent2 = value;
        }
        public string Host
        {
            get => host;
            set => host = value;
        }
        public string Teammate
        {
            get => teammate;
            set => teammate = value;
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
                if (loggedPlayers[i].Login == client)
                    for (int j = 0; j < clients.Count; j++)
                        if (loggedPlayers[i].Hash == clients[j].GetHashCode()) return j;
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
        public void Run(Sqlite sql, string player, List<NetworkStream> clients, List<User> loggedPlayers, List<User> alreadyPlay, List<User> inSolo)
        {
            Host = player;
            NetworkStream c1 = clients[GetClientIndex(player, loggedPlayers, clients)];

            List<string> alreadyAdded = new List<string>();
            Opponent1 = sql.SelectOpponents(player, alreadyPlay, alreadyAdded, inSolo);
            alreadyAdded.Add(Opponent1);
            Teammate = sql.SelectOpponents(player, alreadyPlay, alreadyAdded, inSolo);
            alreadyAdded.Add(Teammate);
            Opponent2 = sql.SelectOpponents(player, alreadyPlay, alreadyAdded, inSolo);
            alreadyAdded.Clear();

            if (Opponent1 != null && Opponent2 != null && Teammate != null)
            {
                MessageTransmission.SendMessage(c1, "Your opponents are: " + Opponent1 + " & " + Opponent2 + Environment.NewLine);
                MessageTransmission.SendMessage(c1, "Your teammate is: " + Teammate + Environment.NewLine);
                MessageTransmission.SendMessage(c1, "Waiting for opponent and teammate..." + Environment.NewLine);

                NetworkStream c2 = clients[GetClientIndex(Opponent1, loggedPlayers, clients)];
                NetworkStream c3 = clients[GetClientIndex(Opponent2, loggedPlayers, clients)];
                NetworkStream c4 = clients[GetClientIndex(Teammate, loggedPlayers, clients)];

                MessageTransmission.SendMessage(c2, "PLAY:");
                MessageTransmission.SendMessage(c3, "PLAY:");
                MessageTransmission.SendMessage(c4, "PLAY:");


                int s = 0;

                while (s < 100)
                {
                    Thread.Sleep(100);
                    s++;
                    if (s == 100 || (opponent1Answere != "" && opponent2Answere != "" && TeammateAnswere != "")) break;
                }

                if (opponent1Answere == "y" && opponent2Answere == "y" && TeammateAnswere == "y" && s < 100)
                {
                    
                playagian:
                    Opponent1Choice.Clear();
                    Opponent2Choice.Clear();
                    TeammateChoice.Clear();
                    PlayerChoice.Clear();

                    for (int i = 1; i <= 4; i++)
                    {
                        Choose(c1);
                        while ((this.Opponent1Choice.Count < i || this.Opponent2Choice.Count < i || this.TeammateChoice.Count < i) && connected) ;
                        if (!connected)
                        {
                            Close(c1);
                            return;
                        }

                        if (Compare(PlayerChoice[i - 1], TeammateChoice[i - 1], Opponent1Choice[i - 1], Opponent2Choice[i - 1]) == 1)
                        {
                            PlayerPoints++;
                            MessageTransmission.SendMessage(c1, "ROUND " + i.ToString() + ": " + player + " & " + Teammate + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                            MessageTransmission.SendMessage(c2, "ROUND " + i.ToString() + ": " + player + " & " + Teammate + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                            MessageTransmission.SendMessage(c3, "ROUND " + i.ToString() + ": " + player + " & " + Teammate + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                            MessageTransmission.SendMessage(c4, "ROUND " + i.ToString() + ": " + player + " & " + Teammate + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        }
                        else if (Compare(PlayerChoice[i - 1], TeammateChoice[i - 1], Opponent1Choice[i - 1], Opponent2Choice[i - 1]) == 2)
                        {
                            OpponentPoints++;
                            MessageTransmission.SendMessage(c1, "ROUND " + i.ToString() + ": " + Opponent1 + " & " + Opponent2 + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                            MessageTransmission.SendMessage(c2, "ROUND " + i.ToString() + ": " + Opponent1 + " & " + Opponent2 + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                            MessageTransmission.SendMessage(c3, "ROUND " + i.ToString() + ": " + Opponent1 + " & " + Opponent2 + " wins! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                            MessageTransmission.SendMessage(c4, "ROUND " + i.ToString() + ": " + Opponent1 + " & " + Opponent2 + " wins! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        }
                        else
                        {
                            OpponentPoints++;
                            PlayerPoints++;
                            MessageTransmission.SendMessage(c1, "ROUND " + i.ToString() + ": Draw! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                            MessageTransmission.SendMessage(c2, "ROUND " + i.ToString() + ": Draw! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                            MessageTransmission.SendMessage(c3, "ROUND " + i.ToString() + ": Draw! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine);
                            MessageTransmission.SendMessage(c4, "ROUND " + i.ToString() + ": Draw! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine);
                        }
                        sql.AddMove(player, PlayerChoice[i - 1]);
                        sql.AddMove(Opponent1, Opponent1Choice[i - 1]);
                        sql.AddMove(Opponent2, Opponent2Choice[i - 1]);
                        sql.AddMove(Teammate, TeammateChoice[i - 1]);
                        MessageTransmission.SendMessage(c1, Environment.NewLine);

                        sql.AchievementUpdate(c1, player);
                    }

                    if (OpponentPoints > PlayerPoints)
                    {
                        sql.AddWin(Opponent1);
                        sql.AddWin(Opponent2);
                        sql.AddLose(player);
                        sql.AddLose(teammate);
                        sql.SetNewRanking(Opponent1, player, 1);
                        sql.SetNewRanking(Opponent2, teammate, 1);
                        sql.SetNewRanking(player, Opponent1, 0);
                        sql.SetNewRanking(teammate, Opponent1, 0);
                        MessageTransmission.SendMessage(c1, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, Environment.NewLine);
                        MessageTransmission.SendMessage(c3, Environment.NewLine);
                        MessageTransmission.SendMessage(c4, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "You won the match! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c3, "You won the match! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c1, "You lost the match! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c4, "You lost the match! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine + Environment.NewLine);

                        MessageTransmission.SendMessage(c2, "STATYou won the match! " + OpponentPoints + ":" + PlayerPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Opponent1) + Environment.NewLine + "Do you want to play again?");
                        MessageTransmission.SendMessage(c3, "STATYou won the match! " + OpponentPoints + ":" + PlayerPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Opponent2) + Environment.NewLine + "Do you want to play again?");
                        MessageTransmission.SendMessage(c1, "STATYou lost the match! " + PlayerPoints + ":" + OpponentPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Host) + Environment.NewLine + "Do you want to play again?");
                        MessageTransmission.SendMessage(c4, "STATYou lost the match! " + PlayerPoints + ":" + OpponentPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Teammate) + Environment.NewLine + "Do you want to play again?");
                    }
                    else if (OpponentPoints < PlayerPoints)
                    {
                        sql.AddWin(player);
                        sql.AddWin(Teammate);
                        sql.AddLose(Opponent1);
                        sql.AddLose(Opponent2);
                        sql.SetNewRanking(Opponent1, player, 0);
                        sql.SetNewRanking(Opponent2, Teammate, 0);
                        sql.SetNewRanking(player, Opponent1, 1);
                        sql.SetNewRanking(Teammate, Opponent2, 1);
                        MessageTransmission.SendMessage(c1, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, Environment.NewLine);
                        MessageTransmission.SendMessage(c3, Environment.NewLine);
                        MessageTransmission.SendMessage(c4, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "You lost the match! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c3, "You lost the match! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c1, "You won the match! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c4, "You won the match! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine + Environment.NewLine);

                        MessageTransmission.SendMessage(c2, "STATYou lost the match! " + OpponentPoints + ":" + PlayerPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Opponent1) + Environment.NewLine + "Do you want to play again?");
                        MessageTransmission.SendMessage(c3, "STATYou lost the match! " + OpponentPoints + ":" + PlayerPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Opponent2) + Environment.NewLine + "Do you want to play again?");
                        MessageTransmission.SendMessage(c1, "STATYou won the match! " + PlayerPoints + ":" + OpponentPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Host) + Environment.NewLine + "Do you want to play again?");
                        MessageTransmission.SendMessage(c4, "STATYou won the match! " + PlayerPoints + ":" + OpponentPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Teammate) + Environment.NewLine + "Do you want to play again?");
                    }
                    else
                    {
                        sql.AddDraw(player);
                        sql.AddDraw(Teammate);
                        sql.AddDraw(Opponent1);
                        sql.AddDraw(Opponent2);
                        sql.SetNewRanking(Opponent1, player, 0.5);
                        sql.SetNewRanking(Opponent2, teammate, 0.5);
                        sql.SetNewRanking(player, Opponent1, 0.5);
                        sql.SetNewRanking(teammate, Opponent2, 0.5);
                        MessageTransmission.SendMessage(c1, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, Environment.NewLine);
                        MessageTransmission.SendMessage(c3, Environment.NewLine);
                        MessageTransmission.SendMessage(c4, Environment.NewLine);
                        MessageTransmission.SendMessage(c2, "You draw the match! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c3, "You draw the match! " + PlayerPoints + ":" + OpponentPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c1, "You draw the match! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine + Environment.NewLine);
                        MessageTransmission.SendMessage(c4, "You draw the match! " + OpponentPoints + ":" + PlayerPoints + Environment.NewLine + Environment.NewLine);

                        MessageTransmission.SendMessage(c2, "STATYou draw the match! " + OpponentPoints + ":" + PlayerPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Opponent1) + Environment.NewLine + "Do you want to play again?");
                        MessageTransmission.SendMessage(c3, "STATYou draw the match! " + OpponentPoints + ":" + PlayerPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Opponent2) + Environment.NewLine + "Do you want to play again?");
                        MessageTransmission.SendMessage(c1, "STATYou draw the match! " + PlayerPoints + ":" + OpponentPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Host) + Environment.NewLine + "Do you want to play again?");
                        MessageTransmission.SendMessage(c4, "STATYou draw the match! " + PlayerPoints + ":" + OpponentPoints
                            + Environment.NewLine + "Your current ELO: " + sql.GetELO(Teammate) + Environment.NewLine + "Do you want to play again?");
                    }
                    opponent1Answere = "";
                    opponent2Answere = "";
                    TeammateAnswere = "";

                    string a1 = MessageTransmission.GetMessage(c1);

                    if (a1 == "y")
                    {
                        PlayerPoints = 0;
                        OpponentPoints = 0;
                        hostAnswere = "y";
                        MessageTransmission.SendMessage(c1, "Waiting for opponent and teammate..." + Environment.NewLine);

                        s = 0;
                        while (s < 100)
                        {
                            Thread.Sleep(100);
                            s++;
                            if (s == 100 || (opponent1Answere != "" && opponent2Answere != "" && TeammateAnswere != "")) break;
                        }

                        if ((opponent1Answere == "y" && opponent2Answere == "y" && TeammateAnswere == "y") && s < 100)
                        {
                            MessageTransmission.SendMessage(c1, "Opponent is ready to play." + Environment.NewLine);
                            goto playagian;
                        }
                        else MessageTransmission.SendMessage(c1, "Opponents left the game." + Environment.NewLine);
                    }
                }
                else
                {
                    this.Opponent1 = "";
                    this.Opponent2 = "";
                    this.Teammate = "";
                    MessageTransmission.SendMessage(c1, "Opponents quit the game. Try to connect again." + Environment.NewLine);
                }
            }
            else MessageTransmission.SendMessage(c1, "There are no other players at the moment." + Environment.NewLine);
        }
        /// <summary>
        /// Prosi gracza o wybór.
        /// </summary>
        /// <param name="Stream">Strumień konkretnego gracza.</param>
        private void Choose(NetworkStream Stream)
        {
            MessageTransmission.SendMessage(Stream, "Choose rock, paper, scissors: " + Environment.NewLine);
            string w = MessageTransmission.GetMessage(Stream);
            PlayerChoice.Add(w);
        }
        /// <summary>
        /// Porównuje wybory gracza i zwraca kto wygrał.
        /// </summary>
        /// <param name="s1">Wybór pierwszego gracza.</param>
        /// <param name="s2">Wybór drugiego gracza.</param>
        /// <returns>Wynik</returns>
        private int Compare(string s1, string s2, string s3, string s4)
        {
            int S1 = 0;
            int S2 = 0;
            int S3 = 0;
            int S4 = 0;

            if (s1 == "r" && s3 == "r") S1++;
            else if (s1 == "s" && s3 == "s") S1++;
            else if (s1 == "p" && s3 == "p") S1++;
            else if (s1 == "r" && s3 == "s") S1++;
            else if (s1 == "p" && s3 == "r") S1++;
            else if (s1 == "s" && s3 == "p") S1++;

            if (s1 == "r" && s4 == "r") S1++;
            else if (s1 == "s" && s4 == "s") S1++;
            else if (s1 == "p" && s4 == "p") S1++;
            else if (s1 == "r" && s4 == "s") S1++;
            else if (s1 == "p" && s4 == "r") S1++;
            else if (s1 == "s" && s4 == "p") S1++;

            if (s2 == "r" && s3 == "r") S2++;
            else if (s2 == "s" && s3 == "s") S2++;
            else if (s2 == "p" && s3 == "p") S2++;
            else if (s2 == "r" && s3 == "s") S2++;
            else if (s2 == "p" && s3 == "r") S2++;
            else if (s2 == "s" && s3 == "p") S2++;

            if (s2 == "r" && s4 == "r") S2++;
            else if (s2 == "s" && s4 == "s") S2++;
            else if (s2 == "p" && s4 == "p") S2++;
            else if (s2 == "r" && s4 == "s") S2++;
            else if (s2 == "p" && s4 == "r") S2++;
            else if (s2 == "s" && s4 == "p") S2++;

            if (s3 == "r" && s1 == "r") S3++;
            else if (s3 == "s" && s1 == "s") S3++;
            else if (s3 == "p" && s1 == "p") S3++;
            else if (s3 == "r" && s1 == "s") S3++;
            else if (s3 == "p" && s1 == "r") S3++;
            else if (s3 == "s" && s1 == "p") S3++;

            if (s3 == "r" && s2 == "r") S3++;
            else if (s3 == "s" && s2 == "s") S3++;
            else if (s3 == "p" && s2 == "p") S3++;
            else if (s3 == "r" && s2 == "s") S3++;
            else if (s3 == "p" && s2 == "r") S3++;
            else if (s3 == "s" && s2 == "p") S3++;

            if (s4 == "r" && s1 == "r") S4++;
            else if (s4 == "s" && s1 == "s") S4++;
            else if (s4 == "p" && s1 == "p") S4++;
            else if (s4 == "r" && s1 == "s") S4++;
            else if (s4 == "p" && s1 == "r") S4++;
            else if (s4 == "s" && s1 == "p") S4++;

            if (s4 == "r" && s2 == "r") S4++;
            else if (s4 == "s" && s2 == "s") S4++;
            else if (s4 == "p" && s2 == "p") S4++;
            else if (s4 == "r" && s2 == "s") S4++;
            else if (s4 == "p" && s2 == "r") S4++;
            else if (s4 == "s" && s2 == "p") S4++;

            if (S1 + S2 > S3 + S4) return 1;
            else if (S1 + S2 < S3 + S4) return 2;
            else return 0;
        }
        private void Close(NetworkStream c1)
        {
            MessageTransmission.SendMessage(c1, Environment.NewLine);
            MessageTransmission.SendMessage(c1, "Opponents quit the game. Try to connect again." + Environment.NewLine);
            MessageTransmission.SendMessage(c1, Environment.NewLine);
        }
    }
}