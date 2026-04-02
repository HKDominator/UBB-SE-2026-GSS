using Events_GSS.Data.Models;

namespace Events_GSS.Data.Services.reputationService;

public interface IReputationService
{
    Task<int> GetReputationPointsAsync(int userId);
    Task<string> GetTierAsync(int userId);
    Task<List<Achievement>> GetAchievementsAsync(int userId);
}
