namespace ServerLibrary
{
    public class User
    {
        private int id;
        private string login;
        private string password;
        private int elo;
        private int logged;
        private int hash;

        /// <summary>
        /// Dane użytkownika w bazie danych.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <param name="login">login.</param>
        /// <param name="password">hasło.</param>
        public User(int id, string login, string password, int elo, int logged)
        {
            this.id = id;
            this.login = login;
            this.password = password;
            this.elo = elo;
            this.logged = logged;
        }

        public int ID
        {
            get => id;
            set => id = value;
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
    }
}
