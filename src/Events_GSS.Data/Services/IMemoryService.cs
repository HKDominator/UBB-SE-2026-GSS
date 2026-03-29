using System.Collections.Generic;
using System.Threading.Tasks;

using Events_GSS.Data.Models;

namespace Events_GSS.Data.Services {

    public interface IMemoryService
    {
        Task<List<Memory>> GetByEventAsync(int eventId, int userId);
        Task<List<string>> GetOnlyPhotosAsync(int eventId);
        Task<List<Memory>> FilterByMyMemoriesAsync(int eventId, int userId);
        Task<List<Memory>> OrderByDateAsync(int eventId, int currentUserId, bool ascending);
        Task AddAsync(int eventId, int userId, string? photoPath, string? text);
        Task DeleteAsync(int memoryId, int userId);
        Task ToggleLikeAsync(int memoryId, int userId);
    }

}