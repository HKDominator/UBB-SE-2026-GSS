using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Events_GSS.Data.Models;
using Events_GSS.Data.Services.notificationServices;
using Events_GSS.Services.Interfaces;

namespace Events_GSS.ViewModels
{
    public class NotificationViewModel
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        private ObservableCollection<Notification> _notifications = new();
        private ObservableCollection<Notification> Notifications
        {
            get { return _notifications; }
            set { _notifications = value; }
        }

        public NotificationViewModel(INotificationService notificationService, IUserService userService)
        {
            _userService = userService;
            _notificationService = notificationService;
        }

        public async Task LoadAsync()
        {
            var currentUser = _userService.GetCurrentUser();
            var notifications = await _notificationService.GetNotificationsAsync(currentUser.UserId);
            Notifications = new ObservableCollection<Notification>(notifications);
        }

        public async Task DeleteAsync(Notification notification)
        {
            await _notificationService.DeleteAsync(notification.Id);
            Notifications.Remove(notification);
        }
    }
}
