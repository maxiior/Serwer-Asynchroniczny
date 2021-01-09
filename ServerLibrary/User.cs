namespace ServerLibrary
{
    public class User
    {
        private string login;
        private string password;
        private int elo;
        private int logged;
        private int hash;
        private int tcphash;

        /// <summary>
        /// Dane użytkownika w bazie danych.
        /// </summary>
        /// <param name="login">login.</param>
        /// <param name="password">hasło.</param>
        /// <param name="elo">wartość elo.</param>
        /// <param name="logged">status zalogowania.</param>
        public User(string login, string password, int elo, int logged)
        {
            this.login = login;
            this.password = password;
            this.elo = elo;
            this.logged = logged;
        }

        public string Login
        {
            get => login;
            set => login = value;
        }
        public string Password
        {
            get => password;
            set => password = value;
        }
        public int Elo
        {
            get => elo;
            set => elo = value;
        }
        public int Logged
        {
            get => logged;
            set => logged = value;
        }
        public int Hash
        {
            get => hash;
            set => hash = value;
        }
        public int TCPHash
        {
            get => tcphash;
            set => tcphash = value;
        }
    }
}
