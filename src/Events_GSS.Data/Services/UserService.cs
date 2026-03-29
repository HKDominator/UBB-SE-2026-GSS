using System.Collections.Generic;
using System.Linq;

using Events_GSS.Data.Models;
using Events_GSS.Data.Services;

namespace Events_GSS.Data.Services
{
    public class UserService : IUserService
    {
        private static readonly List<User> _mockUsers = new()
        {
            new User { UserId = 1, Name = "Alice Admin" },
            new User { UserId = 2, Name = "Bob User" },
            new User { UserId = 3, Name = "Carol User" }
        };

        private static int _currentUserId = 1;

        public User GetCurrentUser() => _mockUsers.First(u => u.UserId == _currentUserId);

        public static void SetCurrentUser(int userId) => _currentUserId = userId;
    }
}