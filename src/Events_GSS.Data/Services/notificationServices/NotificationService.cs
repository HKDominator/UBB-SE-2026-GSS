using System;
using System.Collections.Generic;
using System.Text;

using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories.notificationRepository;

namespace Events_GSS.Data.Services.notificationServices
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        public async Task NotifyAsync(int userId, string title, string description)
        {

        }
        public async Task<List<Notification>> GetNotificationsAsync(int userId)
        {
            return null;
        }
        
        public async Task DeleteAsync(int notificationId)
        {

        }
    }
}
