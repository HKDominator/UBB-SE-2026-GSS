using Events_GSS.Data.Models;

namespace Events_GSS.Data.Services
{
    public interface IUserService
    {
        User GetCurrentUser();
    }
}