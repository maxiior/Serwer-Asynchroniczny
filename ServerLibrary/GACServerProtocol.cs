﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace ServerLibrary
{
    //GameAndCommunicationServerProtocol
    public class GACServerProtocol : CommunicationProtocol
    {
        private DateTime now = DateTime.Now;

        private Sqlite sql = new Sqlite();

        private List<NetworkStream> clients = new List<NetworkStream>();
        private List<Communicator> activeConversation = new List<Communicator>();

        private List<Game> activeGames = new List<Game>();
        private List<User> busyUsers = new List<User>();
        private List<User> loggedPlayers = new List<User>();

        public GACServerProtocol()
        {
            sql.ClearDbLoggedColumn();
        }

        /// <summary>
        /// Funkcja zwracająca nazwę protokołu.
        /// </summary>
        /// <returns>Nazwa protokołu.</returns>
        public override string GetName()
        {
            return "Game And Communication Server Protocol";
        }
        /// <summary>
        /// Usuniecie klienta z pamieci i zmienienie statusu na niezalogowany w przypadku wyłączenia aplikacji klienckiej.
        /// </summary>
        /// <param name="client">Klient.</param>
        public override void Close(TcpClient client)
        {
            foreach(User u in loggedPlayers)
            {
                if(u.TCPHash == client.GetHashCode())
                {
                    sql.LoginStatus(u, 0);

                    busyUsers.Remove(u);
                    loggedPlayers.Remove(u);

                    foreach(Communicator c in activeConversation)
                        if (c.Interlocutor == u.Login || c.Host == u.Login) c.connected = false;

                    foreach (Game g in activeGames)
                        if (g.Opponent == u.Login || g.Host == u.Login) g.connected = false;
                }
            }
        }
        /// <summary>
        /// Menu wyboru opcji użytkownika.
        /// </summary>
        /// <param name="Stream">Strumień użytkownika.</param>
        public override void Run(NetworkStream Stream, TcpClient client)
        {
            Stream.ReadTimeout = 60000;

            string option;

            while (true)
            {
                try
                {
                    MessageTransmission.SendMessage(Stream, "1 - Login" + Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, "2 - Register" + Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, "3 - Show all logins" + Environment.NewLine);

                    option = MessageTransmission.GetMessage(Stream);

                    switch (option)
                    {
                        case "1":
                            login(Stream, client);
                            break;
                        case "2":
                            register(Stream);
                            break;
                        case "3":
                            sql.ShowAllUsers(Stream);
                            break;
                        default:
                            MessageTransmission.SendMessage(Stream, Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "There is no such option. Try again." + Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, Environment.NewLine);
                            break;
                    }
                }
                catch (IOException)
                {
                    return;
                }
            }
        }
        /// <summary>
        /// Okno rejestracji użytkownika.
        /// </summary>
        /// <param name="Stream">Strumień użytkownika.</param>
        private void register(NetworkStream Stream)
        {
            string login, password;
            User u;

            try
            {
                MessageTransmission.SendMessage(Stream, "Enter LOGIN [6-20 signs]: ");
                login = MessageTransmission.GetMessage(Stream);
                MessageTransmission.SendMessage(Stream, "Enter PASSWORD [6-20 signs]: ");
                password = MessageTransmission.GetMessage(Stream);

                u = new User(0, login, password, 500, 0);

                if (u.Login.Length < 6 || u.Login.Length > 25)
                {
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, "Invalid username length." + Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                }
                else if (u.Login.Length < 6 || u.Login.Length > 25)
                {
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, "Invalid password length." + Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                }
                else if (!sql.CheckIfInDB(u))
                {
                    sql.AddUser(u);
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, "Registration was successful." + Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                }
                else
                {
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, "User with that name already exists." + Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, "Try again." + Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                }
            }
            catch (IOException)
            {
                return;
            }
        }
        /// <summary>
        /// Okno logowania użytkownika.
        /// </summary>
        /// <param name="Stream">Strumień użytkownika.</param>
        private void login(NetworkStream Stream, TcpClient client)
        {
            string login, password;
            User u;

            try
            {
                MessageTransmission.SendMessage(Stream, "LOGIN: ");
                login = MessageTransmission.GetMessage(Stream);
                MessageTransmission.SendMessage(Stream, "PASSWORD: ");
                password = MessageTransmission.GetMessage(Stream);

                u = new User(0, login, password, 0, 0);

                if (sql.VerifyLogin(u) && sql.CheckIfLogged(u)==false)
                {
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                    sql.LoginStatus(u, 1);
                    MessageTransmission.SendMessage(Stream, "Login was successful." + Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                    logged(u, Stream, client);
                }
                else if (sql.CheckIfLogged(u) == true)
                {
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, "User already logged in." + Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                }
                else
                {
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, "Wrong login or password." + Environment.NewLine);
                    MessageTransmission.SendMessage(Stream, Environment.NewLine);
                }
            }
            catch (IOException)
            {
                return;
            }
        }

        /// <summary>
        /// Okno wyboru użytkownika, gdy jest już zalogowany.
        /// </summary>
        /// <param name="u">Dany użytkownik.</param>
        /// <param name="Stream">Strumień użytkownika.</param>
        private void logged(User u, NetworkStream Stream, TcpClient client)
        {
            string option;

            u.Hash = Stream.GetHashCode();
            u.TCPHash = client.GetHashCode();
            clients.Add(Stream);
            loggedPlayers.Add(u);

            MessageTransmission.SendMessage(Stream, Environment.NewLine);
            MessageTransmission.SendMessage(Stream, "Logged as: " + u.Login + Environment.NewLine);

            while (true)
            {
                MessageTransmission.SendMessage(Stream, "1 - Play game" + Environment.NewLine);
                MessageTransmission.SendMessage(Stream, "2 - Show profile" + Environment.NewLine);
                MessageTransmission.SendMessage(Stream, "3 - Communicator" + Environment.NewLine);
                MessageTransmission.SendMessage(Stream, "4 - Delete account" + Environment.NewLine);
                MessageTransmission.SendMessage(Stream, "5 - Change password" + Environment.NewLine);
                MessageTransmission.SendMessage(Stream, "6 - Log out" + Environment.NewLine);

                option = MessageTransmission.GetMessage(Stream);

                switch (option)
                {
                    case "3":
                        Communicator c = new Communicator();
                        activeConversation.Add(c);
                        busyUsers.Add(u);
                        c.Run(sql, u.Login, clients, loggedPlayers, busyUsers);
                        busyUsers.Remove(u);
                        activeConversation.Remove(c);
                        break;
                    case "2":
                        List<int> s = new List<int>();
                        s = sql.GetStatistics(u);

                        float matches = 0;
                        float movements = 0;
                        for (int i = 0; i < 3; i++)
                            matches += s[i];

                        for (int i = 3; i < 6; i++)
                            movements += s[i];

                        MessageTransmission.SendMessage(Stream, Environment.NewLine);
                        MessageTransmission.SendMessage(Stream, "Login: " + u.Login + Environment.NewLine);
                        MessageTransmission.SendMessage(Stream, "ELO: " + s[6] + Environment.NewLine);
                        MessageTransmission.SendMessage(Stream, "*Statistics: " + Environment.NewLine);
                        if (matches == 0)
                        {
                            MessageTransmission.SendMessage(Stream, "Wins: " + s[0] + " (0.0%)" + Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Loses: " + s[1] + " (0.0%)" + Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Draws: " + s[2] + " (0.0%)" + Environment.NewLine);
                        }
                        else
                        {
                            MessageTransmission.SendMessage(Stream, "Wins: " + s[0] + " (" + (float)s[0] / matches * 100 + "%)" + Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Loses: " + s[1] + " (" + (float)s[1] / matches * 100 + "%)" + Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Draws: " + s[2] + " (" + (float)s[2] / matches * 100 + "%)" + Environment.NewLine);
                        }
                        MessageTransmission.SendMessage(Stream, "*The most common movements: " + Environment.NewLine);
                        if (matches == 0)
                        {
                            MessageTransmission.SendMessage(Stream, "Rocks: " + s[3] + " (0.0%)" + Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Scissors: " + s[4] + " (0.0%)" + Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Papers: " + s[5] + " (0.0%)" + Environment.NewLine);
                        }
                        else
                        {
                            MessageTransmission.SendMessage(Stream, "Rocks: " + s[3] + " (" + (float)s[3] / movements * 100 + "%)" + Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Scissors: " + s[4] + " (" + (float)s[4] / movements * 100 + "%)" + Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Papers: " + s[5] + " (" + (float)s[5] / movements * 100 + "%)" + Environment.NewLine);
                        }
                        MessageTransmission.SendMessage(Stream, Environment.NewLine);
                        break;
                    case "4":
                        sql.DeleteUser(u);
                        MessageTransmission.SendMessage(Stream, Environment.NewLine);
                        MessageTransmission.SendMessage(Stream, "Account has been deleted." + Environment.NewLine);
                        MessageTransmission.SendMessage(Stream, Environment.NewLine);
                        return;
                    case "5":
                        string newPass;
                        MessageTransmission.SendMessage(Stream, "Enter a new password [6-20 signs]: ");
                        newPass = MessageTransmission.GetMessage(Stream);
                        if (newPass.Length > 5 || 21 < newPass.Length)
                        {
                            sql.ChangePassword(u, newPass);
                            MessageTransmission.SendMessage(Stream, Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Password was changed successfully." + Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, Environment.NewLine);
                        }
                        else
                        {
                            MessageTransmission.SendMessage(Stream, Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Wrong password. Password should be longer than 6 signs." + Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, Environment.NewLine);
                        }
                        break;
                    case "6":
                        MessageTransmission.SendMessage(Stream, Environment.NewLine);
                        loggedPlayers.Remove(u);
                        sql.LoginStatus(u, 0);
                        MessageTransmission.SendMessage(Stream, "You have logged out." + Environment.NewLine);
                        MessageTransmission.SendMessage(Stream, Environment.NewLine);
                        return;
                    case "1":
                        Game g = new Game();
                        activeGames.Add(g);
                        busyUsers.Add(u);
                        g.Run(sql, u.Login, clients, loggedPlayers, busyUsers);
                        busyUsers.Remove(u);
                        activeGames.Remove(g);
                        break;
                    case "y":
                        int gameIndex = -1;
                        for (int i = 0; i < activeGames.Count; i++)
                        {
                            if (activeGames[i].Opponent == u.Login) gameIndex = i;
                        }

                        if (gameIndex != -1)
                        {
                            activeGames[gameIndex].opponentAnswere = "y";
                            busyUsers.Add(u);

                            MessageTransmission.SendMessage(Stream, Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Choose [r]ock, [p]aper, [s]cissors: ");
                            string w = MessageTransmission.GetMessage(Stream);

                            for (int i = 0; i < activeGames.Count; i++)
                                if (activeGames[i].Opponent == u.Login) gameIndex = i;
                            activeGames[gameIndex].OpponentChoice.Add(w);
                            
                            if(activeGames[gameIndex].connected == false)
                            {
                                MessageTransmission.SendMessage(Stream, Environment.NewLine);
                                MessageTransmission.SendMessage(Stream, "The opponent quits the game. Try to connect again." + Environment.NewLine);
                                MessageTransmission.SendMessage(Stream, Environment.NewLine);
                                busyUsers.Remove(u);
                                activeGames.Remove(activeGames[gameIndex]);
                                break;
                            }

                            while (activeGames[gameIndex].PlayerChoice.Count < 1)
                            {
                                for (int i = 0; i < activeGames.Count; i++)
                                    if (activeGames[i].Opponent == u.Login) gameIndex = i;
                            }
                            Thread.Sleep(50);

                            MessageTransmission.SendMessage(Stream, Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Choose [r]ock, [p]aper, [s]cissors: ");
                            w = MessageTransmission.GetMessage(Stream);

                            for (int i = 0; i < activeGames.Count; i++)
                                if (activeGames[i].Opponent == u.Login) gameIndex = i;
                            activeGames[gameIndex].OpponentChoice.Add(w);

                            if (activeGames[gameIndex].connected == false)
                            {
                                MessageTransmission.SendMessage(Stream, Environment.NewLine);
                                MessageTransmission.SendMessage(Stream, "The opponent quits the game. Try to connect again." + Environment.NewLine);
                                MessageTransmission.SendMessage(Stream, Environment.NewLine);
                                busyUsers.Remove(u);
                                activeGames.Remove(activeGames[gameIndex]);
                                break;
                            }

                            while (activeGames[gameIndex].PlayerChoice.Count < 2)
                            {
                                for (int i = 0; i < activeGames.Count; i++)
                                    if (activeGames[i].Opponent == u.Login) gameIndex = i;
                            }
                            Thread.Sleep(50);

                            MessageTransmission.SendMessage(Stream, Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Choose [r]ock, [p]aper, [s]cissors: ");
                            w = MessageTransmission.GetMessage(Stream);

                            for (int i = 0; i < activeGames.Count; i++)
                                if (activeGames[i].Opponent == u.Login) gameIndex = i;
                            activeGames[gameIndex].OpponentChoice.Add(w);

                            if (activeGames[gameIndex].connected == false)
                            {
                                MessageTransmission.SendMessage(Stream, Environment.NewLine);
                                MessageTransmission.SendMessage(Stream, "The opponent quits the game. Try to connect again." + Environment.NewLine);
                                MessageTransmission.SendMessage(Stream, Environment.NewLine);
                                busyUsers.Remove(u);
                                activeGames.Remove(activeGames[gameIndex]);
                                break;
                            }

                            while (activeGames[gameIndex].PlayerChoice.Count < 3)
                            {
                                for (int i = 0; i < activeGames.Count; i++)
                                    if (activeGames[i].Opponent == u.Login) gameIndex = i;
                            }
                            Thread.Sleep(50);

                            MessageTransmission.SendMessage(Stream, Environment.NewLine);
                            MessageTransmission.SendMessage(Stream, "Choose [r]ock, [p]aper, [s]cissors: ");
                            w = MessageTransmission.GetMessage(Stream);

                            for (int i = 0; i < activeGames.Count; i++)
                                if (activeGames[i].Opponent == u.Login) gameIndex = i;
                            activeGames[gameIndex].OpponentChoice.Add(w);

                            if (activeGames[gameIndex].connected == false)
                            {
                                MessageTransmission.SendMessage(Stream, Environment.NewLine);
                                MessageTransmission.SendMessage(Stream, "The opponent quits the game. Try to connect again." + Environment.NewLine);
                                MessageTransmission.SendMessage(Stream, Environment.NewLine);
                                busyUsers.Remove(u);
                                activeGames.Remove(activeGames[gameIndex]);
                                break;
                            }

                            while (activeGames[gameIndex].PlayerChoice.Count < 4)
                            {
                                for (int i = 0; i < activeGames.Count; i++)
                                    if (activeGames[i].Opponent == u.Login) gameIndex = i;
                            }
                            Thread.Sleep(50);

                            busyUsers.Remove(u);
                            Thread.Sleep(300);
                        }

                        int conversationIndex = -1;
                        for (int i = 0; i < activeConversation.Count; i++)
                        {
                            if (activeConversation[i].Interlocutor == u.Login) conversationIndex = i;
                        }

                        if (conversationIndex != -1)
                        {
                            activeConversation[conversationIndex].interlocutorAnswere = "y";
                            busyUsers.Add(u);

                            string m = "";
                            while (activeConversation[conversationIndex].connected && activeConversation[conversationIndex].Interlocutor == u.Login)
                            {
                                try
                                {
                                    if (activeConversation[conversationIndex].connected && activeConversation[conversationIndex].Interlocutor == u.Login)
                                        m = MessageTransmission.GetMessage(Stream);

                                    for (int i = 0; i < activeConversation.Count; i++)
                                        if (activeConversation[i].Interlocutor == u.Login) conversationIndex = i;

                                    if (activeConversation[conversationIndex].connected == false)
                                    {
                                        MessageTransmission.SendMessage(Stream, "The interlocutor ended the conversation." + Environment.NewLine);
                                        break;
                                    }
                                    if (m == "exit")
                                    {
                                        MessageTransmission.SendMessage(Stream, "You have finished the conversation." + Environment.NewLine);
                                        activeConversation[conversationIndex].connected = false;
                                        break;
                                    }

                                    if (m!="" && activeConversation[conversationIndex].Interlocutor == u.Login && activeConversation[conversationIndex].connected)
                                    {
                                        MessageTransmission.SendMessage(activeConversation[conversationIndex].c1, m + " [" + u.Login + "]" + " [" + now.ToString("yyyy-MM-dd hh:mm") + "]" + Environment.NewLine);
                                        sql.AddMessageToDB(u.Login, activeConversation[conversationIndex].Host, m, now.ToString("yyyy-MM-dd hh:mm"));
                                    }
                                }
                                catch (Exception e)
                                {
                                    MessageTransmission.SendMessage(Stream, "The interlocutor ended the conversation." + Environment.NewLine);
                                    break;
                                }
                            }
                            if (!activeConversation[conversationIndex].connected) activeConversation.Remove(activeConversation[conversationIndex]);
                            MessageTransmission.SendMessage(Stream, Environment.NewLine);
                            busyUsers.Remove(u);
                        }
                        break;
                    case "n":
                        gameIndex = -1;
                        for (int i = 0; i < activeGames.Count; i++)
                        {
                            if (activeGames[i].Opponent == u.Login) gameIndex = i;
                        }
                        if (gameIndex != -1) activeGames[gameIndex].opponentAnswere = "n";

                        conversationIndex = -1;
                        for (int i = 0; i < activeConversation.Count; i++)
                        {
                            if (activeConversation[i].Interlocutor == u.Login) gameIndex = i;
                        }
                        if (conversationIndex != -1) activeConversation[conversationIndex].interlocutorAnswere = "n";
                        break;
                    default:
                        MessageTransmission.SendMessage(Stream, Environment.NewLine);
                        break;
                }
            }
        }
    }
}