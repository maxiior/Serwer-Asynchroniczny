using System;
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
            string loginAndPassword;
            User u;

            try
            {
                loginAndPassword = MessageTransmission.GetMessage(Stream);
                string[] tmp = loginAndPassword.Split(' ');
                login = tmp[0];
                password = tmp[1];

                u = new User(login, password, 500, 0);

                if (u.Login.Length < 6 || u.Login.Length > 25) MessageTransmission.SendMessage(Stream, "userlength");
                else if (u.Password.Length < 6 || u.Password.Length > 25) MessageTransmission.SendMessage(Stream, "passlength");
                else if (!sql.CheckIfInDB(u))
                {
                    sql.AddUser(u);
                    MessageTransmission.SendMessage(Stream, "success");
                }
                else MessageTransmission.SendMessage(Stream, "exist");
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
            string loginAndPassword;
            User u;

            try
            {
                loginAndPassword = MessageTransmission.GetMessage(Stream);
                string[] tmp = loginAndPassword.Split(' ');
                login = tmp[0];
                password = tmp[1];
                
                u = new User(login, password, 0, 0);

                if (sql.VerifyLogin(u) && sql.CheckIfLogged(u)==false)
                {
                    sql.LoginStatus(u, 1);
                    MessageTransmission.SendMessage(Stream, "success");
                    logged(u, Stream, client);
                }
                else if (sql.CheckIfLogged(u) == true) MessageTransmission.SendMessage(Stream, "loggedin");
                else MessageTransmission.SendMessage(Stream, "wrong");
            }
            catch (IOException)
            {
                return;
            }
        }
        /// <summary>
        /// Wyznacza i wysyla liste dostepnych do rozmowy osob do użytkownika.
        /// </summary>
        /// <param name="s">Strumień użytkownika, dla którego wyznaczamy listę.</param>
        /// <param name="player">Nazwa użytkownika, dla którego wyznaczamy listę.</param>
        private void GetInterlocutorList(NetworkStream s, string player)
        {
            List<User> users = new List<User>();
            bool b;

            for (int i = 0; i < loggedPlayers.Count; i++)
            {
                b = false;
                for (int j = 0; j < busyUsers.Count; j++)
                {
                    if (loggedPlayers[i] == busyUsers[j])
                    {
                        b = true;
                        break;
                    }
                }
                if (b == false && loggedPlayers[i].Login != player) users.Add(loggedPlayers[i]);
            }

            string logins = "";
            for (int i = 0; i < users.Count; i++)
            {
                if (i != users.Count - 1) logins += users[i].Login + ":";
                else logins += users[i].Login;
            }
            logins = "LOGINS" + logins;
            if(logins!="") MessageTransmission.SendMessage(s, logins);
            else MessageTransmission.SendMessage(s, "noother");
        }
        /// <summary>
        /// Okno wyboru użytkownika, gdy jest już zalogowany.
        /// </summary>
        /// <param name="u">Dany użytkownik.</param>
        /// <param name="Stream">Strumień użytkownika.</param>
        private void logged(User u, NetworkStream Stream, TcpClient client)
        {
            string option;
            busyUsers.Add(u);

            u.Hash = Stream.GetHashCode();
            u.TCPHash = client.GetHashCode();
            clients.Add(Stream);
            loggedPlayers.Add(u);

            while (true)
            {
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

                        string statistics = "";
                        statistics += "LOGIN: " + u.Login + Environment.NewLine;
                        statistics += "ELO: " + s[6] + Environment.NewLine;
                        statistics += Environment.NewLine;
                        statistics += "RESULTS" + Environment.NewLine;

                        if (matches == 0)
                        {
                            statistics += "WINS: " + s[0] + " (0.0%)" + Environment.NewLine;
                            statistics += "LOSES: " + s[1] + " (0.0%)" + Environment.NewLine;
                            statistics += "DRAWS: " + s[2] + " (0.0%)" + Environment.NewLine;
                        }
                        else
                        {
                            statistics += "WINS: " + s[0] + " (" + (float)s[0] / matches * 100 + "%)" + Environment.NewLine;
                            statistics += "LOSES: " + s[1] + " (" + (float)s[1] / matches * 100 + "%)" + Environment.NewLine;
                            statistics += "DRAWS: " + s[2] + " (" + (float)s[2] / matches * 100 + "%)" + Environment.NewLine;
                        }
                        statistics += Environment.NewLine;
                        statistics += "THE MOST COMMON MOVEMENTS" + Environment.NewLine;

                        if (matches == 0)
                        {
                            statistics += "ROCKS: " + s[3] + " (0.0%)" + Environment.NewLine;
                            statistics += "SCISSORS: " + s[4] + " (0.0%)" + Environment.NewLine;
                            statistics += "PAPERS: " + s[5] + " (0.0%)" + Environment.NewLine;
                        }
                        else
                        {
                            statistics += "ROCKS: " + s[3] + " (" + (float)s[3] / movements * 100 + "%)" + Environment.NewLine;
                            statistics += "SCISSORS: " + s[4] + " (" + (float)s[4] / movements * 100 + "%)" + Environment.NewLine;
                            statistics += "PAPERS: " + s[5] + " (" + (float)s[5] / movements * 100 + "%)" + Environment.NewLine;
                        }
                        MessageTransmission.SendMessage(Stream, statistics);
                        break;
                    case "4":
                        sql.DeleteUser(u);
                        return;
                    case "5":
                        string newPass;
                        newPass = MessageTransmission.GetMessage(Stream);
                        if (newPass.Length > 5 || 21 < newPass.Length)
                        {
                            sql.ChangePassword(u, newPass);
                            MessageTransmission.SendMessage(Stream, "success");
                        }
                        else MessageTransmission.SendMessage(Stream, "wrong");
                        break;
                    case "6":
                        loggedPlayers.Remove(u);
                        sql.LoginStatus(u, 0);
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
                        try
                        {
                            for (int i = 0; i < activeGames.Count; i++)
                            {
                                if (activeGames[i].Opponent == u.Login)
                                {
                                    gameIndex = i;
                                    break;
                                }
                                else gameIndex = -1;
                            }
                            if (gameIndex == -1) throw new Exception();

                            if (gameIndex != -1)
                            {
                                activeGames[gameIndex].opponentAnswere = "y";
                                busyUsers.Add(u);

                            playagian:

                                for(int j=1; j<=4; j++)
                                {
                                    MessageTransmission.SendMessage(Stream, "Choose rock, paper, scissors: " + Environment.NewLine);
                                    string w = MessageTransmission.GetMessage(Stream);

                                    for (int i = 0; i < activeGames.Count; i++)
                                    {
                                        if (activeGames[i].Opponent == u.Login)
                                        {
                                            gameIndex = i;
                                            break;
                                        }
                                        else gameIndex = -1;
                                    }
                                    if (gameIndex == -1) throw new Exception();
                                    activeGames[gameIndex].OpponentChoice.Add(w);

                                    if (activeGames[gameIndex].connected == false)
                                    {
                                        MessageTransmission.SendMessage(Stream, "The opponent quits the game. Try to connect again." + Environment.NewLine);
                                        busyUsers.Remove(u);
                                        activeGames.Remove(activeGames[gameIndex]);
                                        break;
                                    }

                                    while (activeGames[gameIndex].PlayerChoice.Count < j)
                                    {
                                        for (int i = 0; i < activeGames.Count; i++)
                                            if (activeGames[i].Opponent == u.Login) gameIndex = i;
                                    }
                                    sql.AchievementUpdate(Stream, u.Login);
                                    Thread.Sleep(50);
                                }

                                string a1 = MessageTransmission.GetMessage(Stream);
                                if (a1 == "y")
                                {
                                    MessageTransmission.SendMessage(Stream, "Waiting for opponent..." + Environment.NewLine);

                                    int se = 0;
                                    activeGames[gameIndex].opponentAnswere = "y";

                                    while (se < 100)
                                    {
                                        Thread.Sleep(100);
                                        se++;
                                        if (se == 100 || activeGames[gameIndex].hostAnswere != "") break;
                                    }

                                    for (int i = 0; i < activeGames.Count; i++)
                                    {
                                        if (activeGames[i].Opponent == u.Login)
                                        {
                                            gameIndex = i;
                                            break;
                                        }
                                        else gameIndex = -1;
                                    }
                                    if (gameIndex == -1) throw new Exception();

                                    if (activeGames[gameIndex].hostAnswere == "y" && se < 100)
                                    {
                                        MessageTransmission.SendMessage(Stream, "Opponent is ready to play." + Environment.NewLine);
                                        goto playagian;
                                    }
                                    else MessageTransmission.SendMessage(Stream, "Opponent left the game." + Environment.NewLine);
                                }
                                else activeGames[gameIndex].opponentAnswere = "n";

                                busyUsers.Remove(u);
                                Thread.Sleep(300);
                            }
                        }
                        catch(Exception)
                        {
                            MessageTransmission.SendMessage(Stream, "Opponent left the game." + Environment.NewLine);
                            busyUsers.Remove(u);
                        }
                        break;
                    case "yc":
                        int conversationIndex = -1;
                        for (int i = 0; i < activeConversation.Count; i++)
                            if (activeConversation[i].Interlocutor == u.Login) conversationIndex = i;

                        if (conversationIndex != -1)
                        {
                            activeConversation[conversationIndex].interlocutorAnswere = "y";
                            busyUsers.Add(u);
                            string m = "";

                            try
                            {
                                while (activeConversation[conversationIndex].connected && activeConversation[conversationIndex].Interlocutor == u.Login)
                                {
                                    if (activeConversation[conversationIndex].connected && activeConversation[conversationIndex].Interlocutor == u.Login)
                                        m = MessageTransmission.GetMessage(Stream);

                                    for (int i = 0; i < activeConversation.Count; i++)
                                        if (activeConversation[i].Interlocutor == u.Login) conversationIndex = i;

                                    if (activeConversation[conversationIndex].connected == false)
                                    {
                                        MessageTransmission.SendMessage(Stream, "quits");
                                        break;
                                    }
                                    if (m == "exit")
                                    {
                                        MessageTransmission.SendMessage(Stream, "finish");
                                        MessageTransmission.SendMessage(activeConversation[conversationIndex].c1, "quits");
                                        activeConversation[conversationIndex].connected = false;
                                        break;
                                    }

                                    if (m != "" && activeConversation[conversationIndex].Interlocutor == u.Login && activeConversation[conversationIndex].connected)
                                    {
                                        MessageTransmission.SendMessage(activeConversation[conversationIndex].c1, m + " [" + u.Login + "]" + " [" + now.ToString("yyyy-MM-dd hh:mm") + "]" + Environment.NewLine);
                                        sql.AddMessageToDB(u.Login, activeConversation[conversationIndex].Host, m, now.ToString("yyyy-MM-dd hh:mm"));
                                    }
                                }
                                if (!activeConversation[conversationIndex].connected) activeConversation.Remove(activeConversation[conversationIndex]);
                                busyUsers.Remove(u);
                            }
                            catch (Exception e)
                            {
                                busyUsers.Remove(u);
                                MessageTransmission.SendMessage(Stream, "quits");
                                break;
                            }
                        }
                        break;
                    case "n":
                        gameIndex = -1;
                        for (int i = 0; i < activeGames.Count; i++)
                            if (activeGames[i].Opponent == u.Login) gameIndex = i;
                        if (gameIndex != -1) activeGames[gameIndex].opponentAnswere = "n";

                        conversationIndex = -1;
                        for (int i = 0; i < activeConversation.Count; i++)
                            if (activeConversation[i].Interlocutor == u.Login) conversationIndex = i;
                        if (conversationIndex != -1) activeConversation[conversationIndex].interlocutorAnswere = "n";
                        break;
                    case "q":
                        MessageTransmission.SendMessage(Stream, "q");
                        busyUsers.Add(u);
                        break;
                    case "nobussy":
                        busyUsers.Remove(u);
                        break;
                    case "freeusers":
                        GetInterlocutorList(Stream, u.Login);
                        break;
                    case "ach":
                        sql.GetAchievements(Stream, u.Login);
                        break;
                }
            }
        }
    }
}