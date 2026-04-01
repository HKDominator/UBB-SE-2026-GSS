using System;
using System.Collections.Generic;
using System.Text;

using Events_GSS.Data.Database;
using Events_GSS.Data.Models;

using Microsoft.Data.SqlClient;

namespace Events_GSS.Data.Repositories.notificationRepository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SqlConnectionFactory _factory;

        public NotificationRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task AddAsync(Notification notification)
        {
            const string query = @"
                INSERT INTO Notifications (Id, UserId, Title, Description, CreatedAt)
                VALUES (@Id, @UserId, @Title, @Description, @CreatedAt)";

            using var connection = _factory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", notification.Id);
            command.Parameters.AddWithValue("@UserId", notification.User.UserId);
            command.Parameters.AddWithValue("@Title", notification.Title);
            command.Parameters.AddWithValue("@Description", notification.Description);
            command.Parameters.AddWithValue("@CreatedAt", notification.CreatedAt);

            await command.ExecuteNonQueryAsync();

        }
        public async Task<List<Notification>> GetByUserIdAsync(int userId)
        {
            return null;
        }

        public async Task DeleteAsync(int notificationId)
        {

        }
    }
}
