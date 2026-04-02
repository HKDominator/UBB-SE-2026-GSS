using System.Collections.Generic;
using System.Threading.Tasks;

using Events_GSS.Data.Models;

namespace Events_GSS.Data.Services.Interfaces
{
    public interface IStatisticsService
    {
        // 10.1
        Task<ParticipantOverview> GetParticipantOverviewAsync(int eventId);

        // 10.2
        Task<EngagementBreakdown> GetEngagementBreakdownAsync(int eventId);

        // 10.3
        Task<List<LeaderboardEntry>> GetLeaderboardAsync(int eventId);

        // 10.4
        Task<List<QuestAnalyticsEntry>> GetQuestAnalyticsAsync(int eventId);
    }
}