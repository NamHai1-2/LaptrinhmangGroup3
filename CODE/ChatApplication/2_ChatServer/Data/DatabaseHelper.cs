using Microsoft.Data.Sqlite;
using System;

namespace _2_ChatServer.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString = "Data Source=chat_database.db";

        public void Initialize()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE,
                    Password TEXT
                );
                CREATE TABLE IF NOT EXISTS Messages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Sender TEXT, Receiver TEXT, Content TEXT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                );
            ";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT OR IGNORE INTO Users (Username, Password) VALUES ('123', '123')";
            cmd.ExecuteNonQuery();
        }

        public bool CheckLogin(string username, string password)
        {
            try
            {
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(1) FROM Users WHERE Username = @u AND Password = @p";
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            catch { return false; }
        }

        public bool RegisterUser(string username, string password)
        {
            try
            {
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();
                var cmd = conn.CreateCommand();

                cmd.CommandText = "INSERT INTO Users (Username, Password) VALUES (@u, @p)";
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}