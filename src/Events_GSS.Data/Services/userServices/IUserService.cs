using Events_GSS.Data.Models;

namespace Events_GSS.Services.Interfaces
{
    public interface IUserService
    {
        User GetCurrentUser();
        User? GetUserById(int userId);
        List<User> GetFriends(int userId);
        List<User> SearchFriends(int userId, string name);
    }
}