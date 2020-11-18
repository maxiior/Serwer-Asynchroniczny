using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
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

                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].Logged == 1) logged = "TRUE";
                    else logged = "FALSE";
                    MessageTransmission.SendMessage(s, string.Concat("LOGIN: ", users[i].Login, Environment.NewLine));
                    MessageTransmission.SendMessage(s, string.Concat("PASSWORD: ", users[i].Password, Environment.NewLine));
                    MessageTransmission.SendMessage(s, string.Concat("ELO: ", users[i].Elo, Environment.NewLine));
                    MessageTransmission.SendMessage(s, string.Concat("LOGGED: ", users[i].Logged, Environment.NewLine));
                    MessageTransmission.SendMessage(s, Environment.NewLine);
                }

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

        private static string ConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
        }
    }
}