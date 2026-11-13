using System;
using System.Collections.Generic;
using System.Data.SQLite;
using UsersApp.Models;

namespace UsersApp.Data
{
    public class MasterRepository
    {
        public List<Master> GetAll(string? searchText = null)
        {
            var search = searchText?.Trim() ?? string.Empty;
            var masters = new List<Master>();

            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT MasterID, FullName, Phone, Specialization
                FROM Masters
                WHERE @search = ''
                   OR FullName LIKE @pattern
                   OR Phone LIKE @pattern
                   OR Specialization LIKE @pattern
                ORDER BY FullName COLLATE NOCASE;";
            command.Parameters.AddWithValue("@search", search);
            command.Parameters.AddWithValue("@pattern", $"%{search}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                masters.Add(new Master
                {
                    MasterID = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Phone = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Specialization = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return masters;
        }

        public void Add(Master master)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Masters (FullName, Phone, Specialization)
                VALUES (@fullName, @phone, @specialization);";
            AddMasterParameters(command, master);
            command.ExecuteNonQuery();
        }

        public void Update(Master master)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Masters
                SET FullName = @fullName,
                    Phone = @phone,
                    Specialization = @specialization
                WHERE MasterID = @masterId;";
            AddMasterParameters(command, master);
            command.Parameters.AddWithValue("@masterId", master.MasterID);

            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Майстра не знайдено.");
            }
        }

        public void Delete(int masterId)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Masters WHERE MasterID = @masterId;";
            command.Parameters.AddWithValue("@masterId", masterId);

            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Майстра не знайдено.");
            }
        }

        private static void AddMasterParameters(SQLiteCommand command, Master master)
        {
            command.Parameters.AddWithValue("@fullName", master.FullName.Trim());
            command.Parameters.AddWithValue("@phone", (object?)master.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@specialization", (object?)master.Specialization ?? DBNull.Value);
        }
    }
}
