using System;
using System.Collections.Generic;
using System.Data.SQLite;
using UsersApp.Models;

namespace UsersApp.Data
{
    public class ClientRepository
    {
        public List<Client> GetAll(string? searchText = null)
        {
            var search = searchText?.Trim() ?? string.Empty;
            var clients = new List<Client>();

            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ClientID, FullName, Phone, Email
                FROM Clients
                WHERE @search = ''
                   OR FullName LIKE @pattern
                   OR Phone LIKE @pattern
                   OR Email LIKE @pattern
                ORDER BY FullName COLLATE NOCASE;";
            command.Parameters.AddWithValue("@search", search);
            command.Parameters.AddWithValue("@pattern", $"%{search}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                clients.Add(new Client
                {
                    ClientID = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Phone = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return clients;
        }

        public void Add(Client client)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Clients (FullName, Phone, Email)
                VALUES (@fullName, @phone, @email);";
            AddClientParameters(command, client);
            command.ExecuteNonQuery();
        }

        public void Update(Client client)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Clients
                SET FullName = @fullName,
                    Phone = @phone,
                    Email = @email
                WHERE ClientID = @clientId;";
            AddClientParameters(command, client);
            command.Parameters.AddWithValue("@clientId", client.ClientID);

            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Клієнта не знайдено або його дані не змінилися.");
            }
        }

        public void Delete(int clientId)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Clients WHERE ClientID = @clientId;";
            command.Parameters.AddWithValue("@clientId", clientId);

            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Клієнта не знайдено.");
            }
        }

        private static void AddClientParameters(SQLiteCommand command, Client client)
        {
            command.Parameters.AddWithValue("@fullName", client.FullName.Trim());
            command.Parameters.AddWithValue("@phone", (object?)client.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@email", (object?)client.Email ?? DBNull.Value);
        }
    }
}
