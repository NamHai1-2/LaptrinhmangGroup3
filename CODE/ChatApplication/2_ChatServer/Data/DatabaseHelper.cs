using Microsoft.Data.Sqlite;

public void Initialize()
{

    using var conn = new SqliteConnection("Data Source=chat_database.db");

    conn.Open();

    var cmd = conn.CreateCommand();

    cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT UNIQUE,
            Password TEXT
        );
    ";
    cmd.ExecuteNonQuery();

    cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Messages (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Sender TEXT,
            Receiver TEXT,
            Content TEXT,
            Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
        );
    ";
    cmd.ExecuteNonQuery();
}
