using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using UsersApp.Models;

namespace UsersApp.Data
{
    public class OrderRepository
    {
        public List<SalonOrder> GetForDate(DateTime date)
        {
            var orders = new List<SalonOrder>();

            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT o.OrderID,
                       o.ClientID,
                       o.MasterID,
                       o.ServiceID,
                       o.OrderDate,
                       o.OrderTime,
                       o.TotalPrice,
                       o.Status,
                       c.FullName,
                       m.FullName,
                       s.ServiceName
                FROM Orders o
                INNER JOIN Clients c ON c.ClientID = o.ClientID
                INNER JOIN Masters m ON m.MasterID = o.MasterID
                INNER JOIN Services s ON s.ServiceID = o.ServiceID
                WHERE o.OrderDate = @orderDate
                ORDER BY o.OrderTime;";
            command.Parameters.AddWithValue("@orderDate", date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                orders.Add(new SalonOrder
                {
                    OrderID = reader.GetInt32(0),
                    ClientID = reader.GetInt32(1),
                    MasterID = reader.GetInt32(2),
                    ServiceID = reader.GetInt32(3),
                    OrderDate = reader.GetString(4),
                    OrderTime = reader.GetString(5),
                    TotalPrice = Convert.ToDecimal(reader.GetValue(6), CultureInfo.InvariantCulture),
                    Status = reader.GetString(7),
                    ClientName = reader.GetString(8),
                    MasterName = reader.GetString(9),
                    ServiceName = reader.GetString(10)
                });
            }

            return orders;
        }

        public List<DateTime> GetDatesWithOrders(DateTime month)
        {
            var dates = new List<DateTime>();

            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT DISTINCT OrderDate
                FROM Orders
                WHERE OrderDate >= @monthStart AND OrderDate < @nextMonth
                ORDER BY OrderDate;";
            command.Parameters.AddWithValue("@monthStart", new DateTime(month.Year, month.Month, 1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@nextMonth", new DateTime(month.Year, month.Month, 1).AddMonths(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (DateTime.TryParseExact(reader.GetString(0), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var date))
                {
                    dates.Add(date.Date);
                }
            }

            return dates;
        }

        public SalonOrder? GetById(int orderId)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT o.OrderID, o.ClientID, o.MasterID, o.ServiceID, o.OrderDate, o.OrderTime,
                       o.TotalPrice, o.Status, c.FullName, m.FullName, s.ServiceName
                FROM Orders o
                INNER JOIN Clients c ON c.ClientID = o.ClientID
                INNER JOIN Masters m ON m.MasterID = o.MasterID
                INNER JOIN Services s ON s.ServiceID = o.ServiceID
                WHERE o.OrderID = @orderId;";
            command.Parameters.AddWithValue("@orderId", orderId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadOrder(reader) : null;
        }

        public List<Master> GetMasters()
        {
            var masters = new List<Master>();

            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT MasterID, FullName, Phone, Specialization
                FROM Masters
                ORDER BY FullName COLLATE NOCASE;";

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

        public List<SalonService> GetServices()
        {
            var services = new List<SalonService>();

            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ServiceID, ServiceName, Description, Price, Duration
                FROM Services
                ORDER BY ServiceName COLLATE NOCASE;";

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

        public void Add(SalonOrder order)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Orders
                    (ClientID, MasterID, ServiceID, OrderDate, OrderTime, TotalPrice, Status)
                VALUES
                    (@clientId, @masterId, @serviceId, @orderDate, @orderTime, @totalPrice, @status);";
            command.Parameters.AddWithValue("@clientId", order.ClientID);
            command.Parameters.AddWithValue("@masterId", order.MasterID);
            command.Parameters.AddWithValue("@serviceId", order.ServiceID);
            command.Parameters.AddWithValue("@orderDate", order.OrderDate);
            command.Parameters.AddWithValue("@orderTime", order.OrderTime);
            command.Parameters.AddWithValue("@totalPrice", order.TotalPrice);
            command.Parameters.AddWithValue("@status", order.Status);
            command.ExecuteNonQuery();
        }

        public void Update(SalonOrder order)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Orders
                SET ClientID = @clientId,
                    MasterID = @masterId,
                    ServiceID = @serviceId,
                    OrderDate = @orderDate,
                    OrderTime = @orderTime,
                    TotalPrice = @totalPrice,
                    Status = @status
                WHERE OrderID = @orderId;";
            AddOrderParameters(command, order);
            command.Parameters.AddWithValue("@orderId", order.OrderID);
            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Замовлення не знайдено.");
            }
        }

        public void UpdateStatus(int orderId, string status)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Orders SET Status = @status WHERE OrderID = @orderId;";
            command.Parameters.AddWithValue("@status", status);
            command.Parameters.AddWithValue("@orderId", orderId);
            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Замовлення не знайдено.");
            }
        }

        public void Delete(int orderId)
        {
            using var connection = BeautySalonConnectionFactory.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Orders WHERE OrderID = @orderId;";
            command.Parameters.AddWithValue("@orderId", orderId);
            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Замовлення не знайдено.");
            }
        }

        private static SalonOrder ReadOrder(SQLiteDataReader reader)
        {
            return new SalonOrder
            {
                OrderID = reader.GetInt32(0),
                ClientID = reader.GetInt32(1),
                MasterID = reader.GetInt32(2),
                ServiceID = reader.GetInt32(3),
                OrderDate = reader.GetString(4),
                OrderTime = reader.GetString(5),
                TotalPrice = Convert.ToDecimal(reader.GetValue(6), CultureInfo.InvariantCulture),
                Status = reader.GetString(7),
                ClientName = reader.GetString(8),
                MasterName = reader.GetString(9),
                ServiceName = reader.GetString(10)
            };
        }

        private static void AddOrderParameters(SQLiteCommand command, SalonOrder order)
        {
            command.Parameters.AddWithValue("@clientId", order.ClientID);
            command.Parameters.AddWithValue("@masterId", order.MasterID);
            command.Parameters.AddWithValue("@serviceId", order.ServiceID);
            command.Parameters.AddWithValue("@orderDate", order.OrderDate);
            command.Parameters.AddWithValue("@orderTime", order.OrderTime);
            command.Parameters.AddWithValue("@totalPrice", order.TotalPrice);
            command.Parameters.AddWithValue("@status", order.Status);
        }
    }
}
