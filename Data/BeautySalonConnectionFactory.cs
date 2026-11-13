using System;
using System.Data.SQLite;
using System.IO;

namespace UsersApp.Data
{
    public static class BeautySalonConnectionFactory
    {
        private const string DatabaseFileName = "BeautySalonAIS.db";

        public static SQLiteConnection CreateOpenConnection()
        {
            var databasePath = Path.Combine(AppContext.BaseDirectory, DatabaseFileName);

            if (!File.Exists(databasePath))
            {
                throw new FileNotFoundException(
                    $"Базу даних салону не знайдено: '{databasePath}'. Переконайтеся, що файл BeautySalonAIS.db розміщений у папці програми.",
                    databasePath);
            }

            var connectionStringBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = databasePath,
                Version = 3,
                FailIfMissing = true
            };

            var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);

            try
            {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();

                command.CommandText = "PRAGMA foreign_keys;";
                if (Convert.ToInt32(command.ExecuteScalar()) != 1)
                {
                    throw new InvalidOperationException(
                        "Не вдалося увімкнути перевірку зовнішніх ключів для SQLite-з'єднання.");
                }

                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }
    }
}
