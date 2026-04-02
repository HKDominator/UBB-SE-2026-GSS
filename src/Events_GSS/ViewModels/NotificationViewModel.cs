using System;
using System.Collections.Generic;
using System.Text;

using Events_GSS.Data.Services.notificationServices;
using Events_GSS.Services.Interfaces;

namespace Events_GSS.ViewModels
{
    public class NotificationViewModel
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;


        public NotificationViewModel(INotificationService notificationService, IUserService userService)
        {
            _userService = userService;
            _notificationService = notificationService;
        }
    }
}
