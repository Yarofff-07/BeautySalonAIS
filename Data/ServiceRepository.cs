using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using UsersApp.Models;

namespace UsersApp.Data
{
    public class ServiceRepository
    {
        public List<SalonService> GetAll(string? searchText = null)
        {
            var search = searchText?.Trim() ?? string.Empty;
            var services = new List<SalonService>();

            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ServiceID, ServiceName, Description, Price, Duration
                FROM Services
                WHERE @search = '' OR ServiceName LIKE @pattern
                ORDER BY ServiceName COLLATE NOCASE;";
            command.Parameters.AddWithValue("@search", search);
            command.Parameters.AddWithValue("@pattern", $"%{search}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                services.Add(new SalonService
                {
                    ServiceID = reader.GetInt32(0),
                    ServiceName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Price = Convert.ToDecimal(reader.GetValue(3), CultureInfo.InvariantCulture),
                    Duration = reader.GetInt32(4)
                });
            }

            return services;
        }

        public void Add(SalonService service)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Services (ServiceName, Description, Price, Duration)
                VALUES (@serviceName, @description, @price, @duration);";
            AddServiceParameters(command, service);
            command.ExecuteNonQuery();
        }

        public void Update(SalonService service)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Services
                SET ServiceName = @serviceName,
                    Description = @description,
                    Price = @price,
                    Duration = @duration
                WHERE ServiceID = @serviceId;";
            AddServiceParameters(command, service);
            command.Parameters.AddWithValue("@serviceId", service.ServiceID);

            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Послугу не знайдено.");
            }
        }

        public void Delete(int serviceId)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Services WHERE ServiceID = @serviceId;";
            command.Parameters.AddWithValue("@serviceId", serviceId);

            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Послугу не знайдено.");
            }
        }

        private static void AddServiceParameters(SQLiteCommand command, SalonService service)
        {
            command.Parameters.AddWithValue("@serviceName", service.ServiceName.Trim());
            command.Parameters.AddWithValue("@description", (object?)service.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@price", service.Price);
            command.Parameters.AddWithValue("@duration", service.Duration);
        }
    }
}
