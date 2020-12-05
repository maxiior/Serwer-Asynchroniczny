using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Net.Sockets;
using System.IO;
using System.Configuration;
using System.Data;
using Dapper;

namespace ServerLibrary
{
    public class Sqlite
    {
        /// <summary>
        /// Funkcja szyfrująca
        /// </summary>
        /// <param name="e">Zwraca zaszyfrowane hasło</param>
        /// <returns></returns>
        public string Encrypt(string e, string l)
        {
            try
            {
                string mySalt = BCrypt.Net.BCrypt.GenerateSalt();
                string fName = l + ".txt";

                using (StreamWriter sw = File.CreateText(fName))
                {
                    sw.WriteLine(mySalt);
                }

                string r = BCrypt.Net.BCrypt.HashPassword(e, mySalt);
                return r;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }
        /// <summary>
        /// Sprawdza zgodność loginu i hasła użytkownika.
        /// </summary>
        /// <param name="u">Obiekt użytkownika.</param>
        /// <returns>Czy login i hasło są zgodne, czy nie</returns>
        private string CheckPassword(User u)
        {
            if (CheckIfInDB(u))
            {
                string fName = u.Login + ".txt";
                string line;

                using (StreamReader sr = new StreamReader(fName))
                {
                    line = sr.ReadLine();
                }

                return BCrypt.Net.BCrypt.HashPassword(u.Password, line);
            }
            else return "text";
        }
        /// <summary>
        /// Zmienia status zalogowania użytkownika w bazie.
        /// </summary>
        /// <param name="u">Nazwa użytkownika.</param>
        /// <param name="x">Czy jest zalogowany, czy nie</param>
        public void LoginStatus(User u, int x)
        {
            string l;
            if (x == 1) l = "1";
            else l = "0";
            u.Logged = x;

            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                DbConn.Execute($"UPDATE Users SET logged={l} WHERE login='{u.Login}';");
            }
        }
        /// <summary>
        /// Sprawdzamy, czy hasło i login podczas logowania są prawidłowe.
        /// </summary>
        /// <param name="u">Hasło i login do sprawdzenia.</param>
        /// <returns>True lub false w zależności, czy dane są prawidłowe.</returns>
        public bool VerifyLogin(User u)
        {
            List<string> user = new List<string>();
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                user = DbConn.Query<string>($"SELECT Login FROM Users WHERE Login='{u.Login}' AND Password='{CheckPassword(u)}';", new DynamicParameters()).ToList();

                if (user.Count == 1) return true;
                else return false;
            }
        }
        /// <summary>
        /// Zmienia hasło użytkownika.
        /// </summary>
        /// <param name="u">Użytkownika, dla kótrgo zmieniamy hasło.</param>
        /// <param name="pass">Nowe hasło.</param>
        public void ChangePassword(User u, string pass)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                DbConn.Execute($"UPDATE Users SET password='{Encrypt(pass, u.Login)}' WHERE login='{u.Login}';");
            }
        }
        /// <summary>
        /// Dodaje użytkownika do bazy danych.
        /// </summary>
        /// <param name="u">Użytkownik do dodania.</param>
        public void AddUser(User u)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                DbConn.Execute($"INSERT INTO Users (login, password, elo, logged) VALUES ('{u.Login}', '{Encrypt(u.Password, u.Login)}', '{u.Elo}',0);");
            }
        }
        /// <summary>
        /// Usuwa użytkownika z bazy danych.
        /// </summary>
        /// <param name="u">Użytkownik do usunięcia.</param>
        public void DeleteUser(User u)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                DbConn.Execute($"DELETE FROM Users WHERE login='{u.Login}';");
            }
        }
        /// <summary>
        /// Sprawdza, czy użytkownik jest w bazie danych.
        /// </summary>
        /// <param name="u">Użytkownik do sprawdzenia.</param>
        /// <returns></returns>
        public bool CheckIfInDB(User u)
        {
            List<string> user = new List<string>();
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                user = DbConn.Query<string>($"SELECT Login FROM Users WHERE Login='{u.Login}';", new DynamicParameters()).ToList();

                if (user.Count == 1) return true;
                else return false;
            }
        }
        /// <summary>
        /// Ustawianie elo gracza player względem wyniku meczu.
        /// </summary>
        /// <param name="player">Gracz</param>
        /// <param name="opponent">Jego rywal</param>
        /// <param name="result">Rezultat meczu</param>
        public void SetNewRanking(string player, string opponent, double result)
        {
            //result - 1 pkt za zwycięstwo, 0.5 za remis i 0 za porażkę

            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                var PlayerELO = DbConn.QuerySingleOrDefault($"SELECT Elo FROM Users WHERE login='{player}';");
                double actualA = PlayerELO.Elo;

                var OpponentELO = DbConn.QuerySingleOrDefault($"SELECT Elo FROM Users WHERE login='{opponent}';");
                double actualB = OpponentELO.Elo;

                double pkt;

                double We = 1 / (Math.Pow(10, -(actualA - actualB) / 300) + 1);

                pkt = actualA + 30 * (result - We);

                string value = Convert.ToInt32(Math.Ceiling(pkt)).ToString();

                DbConn.Execute($"UPDATE Users SET elo={value} WHERE login='{player}';");
            }
        }
        /// <summary>
        /// Sprawdza, czy dany gracz juz gra.
        /// </summary>
        /// <param name="player">Gracz</param>
        /// <param name="alreadyPlay">Gracze, którzy teraz grają.</param>
        /// <returns>Czy gracz gra, czy nie</returns>
        public bool IfAlreadyPlay(string player, List<User> alreadyPlay, string me)
        {
            for (int i = 0; i < alreadyPlay.Count; i++)
            {
                if (player == alreadyPlay[i].Login && player != me) return true;
            }
            return false;
        }
        /// <summary>
        /// Wyszukuje najlepiej dopasowanego przeciwnika wśród zalogowanych.
        /// </summary>
        /// <param name="player">Nazwa gracza.</param>
        /// <returns>Nazwa przeciwnika</returns>
        public string SelectOpponent(string player, List<User> alreadyPlay)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                var PlayerELO = DbConn.QuerySingleOrDefault($"SELECT Elo FROM Users WHERE login='{player}';");
                int actualA = Convert.ToInt32(PlayerELO.Elo);

                var players = DbConn.Query("SELECT Login, Elo, Logged FROM Users;", new { ids = new[] { 1, 2, 3 } }).ToList(); ;

                List<string> p = new List<string>();
                List<int> e = new List<int>();

                for (int i = 0; i < players.Count; i++)
                {
                    if (Convert.ToInt32(players[i].Logged) == 1 && players[i].Login != player && IfAlreadyPlay(players[i].Login, alreadyPlay, player) == false)
                    {
                        p.Add(players[i].Login);
                        e.Add(Convert.ToInt32(players[i].Elo));
                    }
                }

                if (e.Count() > 0)
                {
                    int min = Math.Abs(e[0] - actualA);
                    int index = 0;

                    for (int i = 1; i < e.Count(); i++)
                    {
                        if (Math.Abs(e[i] - actualA) < min)
                        {
                            min = e[i];
                            index = i;
                        }
                    }
                    return p[index];
                }
                else return null;

            }
        }
        /// <summary>
        /// Wyświetla wszystkie dane w bazie danych.
        /// </summary>
        /// <param name="s">Strumień</param>
        public void ShowAllUsers(NetworkStream s)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                var users = DbConn.Query("SELECT * FROM Users;", new { ids = new[] { 1, 2, 3, 4, 5 } }).ToList();

                string logged;

                string u = "";

                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].Logged == 1) logged = "TRUE";
                    else logged = "FALSE";

                    u += string.Concat("LOGIN: ", users[i].Login, Environment.NewLine);
                    u += string.Concat("PASSWORD: ", users[i].Password, Environment.NewLine);
                    u += string.Concat("ELO: ", users[i].Elo, Environment.NewLine);
                    u += string.Concat("LOGGED: ", users[i].Logged, Environment.NewLine);
                    u += Environment.NewLine;
                }
                MessageTransmission.SendMessage(s, u);

            }
        }
        /// <summary>
        /// Zmienia status zalogowania użytkownika w bazie.
        /// </summary>
        /// <param name="u">Nazwa użytkownika.</param>
        /// <param name="x">Czy jest zalogowany, czy nie</param>
        public void ClearDbLoggedColumn()
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                DbConn.Execute($"UPDATE Users SET logged=0;");
            }
        }
        /// <summary>
        /// Dodaje wygraną do statystyk użytkownika.
        /// </summary>
        /// <param name="login">Login użytkownika</param>
        public void AddWin(string login)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                DbConn.Execute($"UPDATE Users SET Wins = Wins+1 WHERE login='{login}';");
            }
        }
        /// <summary>
        /// Dodaje przegraną do statystyk użytkownika.
        /// </summary>
        /// <param name="login">Login użytkownika</param>
        public void AddLose(string login)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                DbConn.Execute($"UPDATE Users SET Loses = Loses+1 WHERE login='{login}';");
            }
        }
        /// <summary>
        /// Dodaje remis do statystyk użytkownika.
        /// </summary>
        /// <param name="login">Login użytkownika</param>
        public void AddDraw(string login)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                DbConn.Execute($"UPDATE Users SET Draws = Draws+1 WHERE login='{login}';");
            }
        }
        /// <summary>
        /// Pobiera statystyki gracza z bazy danych.
        /// </summary>
        /// <param name="u">Gracz.</param>
        /// <returns>Zwraca statystyki gracza.</returns>
        public List<int> GetStatistics(User u)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                var st = DbConn.Query($"SELECT Wins, Loses, Draws, Rocks, Scissors, Papers, Elo FROM Users WHERE login='{u.Login}';", new { ids = new[] { 1, 2, 3, 4, 5, 6, 7 } }).ToList();

                List<int> s = new List<int>();
                s.Add(Convert.ToInt32(st[0].Wins));
                s.Add(Convert.ToInt32(st[0].Loses));
                s.Add(Convert.ToInt32(st[0].Draws));
                s.Add(Convert.ToInt32(st[0].Rocks));
                s.Add(Convert.ToInt32(st[0].Scissors));
                s.Add(Convert.ToInt32(st[0].Papers));
                s.Add(Convert.ToInt32(st[0].Elo));

                return s;
            }
        }
        /// <summary>
        /// Dodaje ruchy do statystyk użytkownika.
        /// </summary>
        /// <param name="login">Nazwa uzytkownika.</param>
        /// <param name="move">Ruch.</param>
        public void AddMove(string login, string move)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                if (move == "r") DbConn.Execute($"UPDATE Users SET Rocks = Rocks+1 WHERE login='{login}';");
                else if(move == "s") DbConn.Execute($"UPDATE Users SET Scissors = Scissors+1 WHERE login='{login}';");
                else DbConn.Execute($"UPDATE Users SET Papers = Papers+1 WHERE login='{login}';");
            }
        }
        /// <summary>
        /// Funkcja sprawdzająca, czy użytkownik jest zalogowany
        /// </summary>
        /// <returns>Zwraca true jeżeli jest zalogowany i false jeżeli nie jest zalogowany</returns>
        public bool CheckIfLogged(User u)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                try
                {
                    var logged = DbConn.QuerySingleOrDefault($"SELECT Logged FROM Users WHERE Login='{u.Login}';");
                    int l = Convert.ToInt32(logged.Logged);

                    if (l == 1) return true;
                    else return false;
                }
                catch(Exception e)
                {
                    return false;
                }
                
            }
        }
        /// <summary>
        /// Dodaje konwersację do bazy danych.
        /// </summary>
        /// <param name="u1">Login pierwszego gracza.</param>
        /// <param name="u2">Login drugiego gracza.</param>
        public void AddConversationToDB(string u1, string u2)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                var st = DbConn.Query($"SELECT user1, user2 FROM Conversations;", new { ids = new[] { 1, 2 } }).ToList();

                bool check = false;

                for(int i=0; i<st.Count(); i++)
                    if ((st[i].user1 == u1 && st[i].user2 == u2) || (st[i].user1 == u2 && st[i].user2 == u1)) check = true;

                if(!check) DbConn.Execute($"INSERT INTO Conversations (user1, user2) VALUES ('{u1}', '{u2}');");
            }
        }
        /// <summary>
        /// Zapisuje wiadomość w bazie danych sqlite.
        /// </summary>
        /// <param name="m">Wiadomość.</param>
        /// <param name="d">Data wysłania.</param>
        public void AddMessageToDB(string u1, string u2, string m, string d)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                var st = DbConn.Query($"SELECT converId, user1, user2 FROM Conversations;", new { ids = new[] { 1, 2, 3  } }).ToList();

                bool check = false;
                int id = 0;
                
                for (int i = 0; i < st.Count(); i++)
                    if ((st[i].user1 == u1 && st[i].user2 == u2) || (st[i].user1 == u2 && st[i].user2 == u1))
                    {
                        check = true;
                        id = Convert.ToInt32(st[i].converId);
                    }

                if (check)
                {
                    DbConn.Execute($"INSERT INTO Messages (converId, date, message, sender) VALUES ('{id}', '{d}', '{m}', '{u1}');");
                }
            }
        }
        /// <summary>
        /// Wczytuje historię konwersacji użytkowników.
        /// </summary>
        /// <param name="u1">Pierwszy użytkownik.</param>
        /// <param name="u2">Drugi użytkownik.</param>
        public string ReadMessagesHistoryDB(string u1, string u2)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                try
                {
                    var st = DbConn.Query($"SELECT converId, user1, user2 FROM Conversations;", new { ids = new[] { 1, 2, 3 } }).ToList();

                    bool check = false;
                    int id = 0;

                    for (int i = 0; i < st.Count(); i++)
                        if ((st[i].user1 == u1 && st[i].user2 == u2) || (st[i].user1 == u2 && st[i].user2 == u1))
                        {
                            check = true;
                            id = Convert.ToInt32(st[i].converId);
                        }

                    if (check)
                    {
                        var data = DbConn.Query($"SELECT date, message, sender FROM Messages WHERE converId={id};", new { ids = new[] { 1, 2, 3 } }).ToList();

                        string messages = "";

                        for (int i = 0; i < data.Count(); i++)
                            messages += data[i].sender + " " + "[" + data[i].date + "]" + " " + data[i].message + Environment.NewLine;

                        return messages;
                    }
                    else return "";
                }
                catch(Exception e)
                {
                    return "";
                }

            }
        }
        /// <summary>
        /// Zwraca punktacje ELO gracza.
        /// </summary>
        /// <param name="login">Nazwa gracza.</param>
        /// <returns>ELO gracza.</returns>
        public string GetELO(string login)
        {
            using (IDbConnection DbConn = new SQLiteConnection(ConnectionString()))
            {
                try
                {
                    var st = DbConn.QuerySingleOrDefault($"SELECT Elo FROM Users WHERE Login='{login}';");

                    string r = Convert.ToString(st.Elo);

                    return r;
                }
                catch (Exception e)
                {
                    return "";
                }

            }
        }

        private static string ConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
        }
    }
}