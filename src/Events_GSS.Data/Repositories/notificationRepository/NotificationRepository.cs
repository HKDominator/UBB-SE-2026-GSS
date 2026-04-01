using System;
using System.Collections.Generic;
using System.Text;

using Events_GSS.Data.Database;
using Events_GSS.Data.Models;

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
