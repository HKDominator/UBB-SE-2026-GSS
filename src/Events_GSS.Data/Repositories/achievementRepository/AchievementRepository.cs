using Events_GSS.Data.Database;
using Events_GSS.Data.Models;
using Microsoft.Data.SqlClient;

namespace Events_GSS.Data.Repositories.achievementRepository;

public class AchievementRepository : IAchievementRepository
{
    private readonly SqlConnectionFactory _factory;

    public AchievementRepository(SqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<Achievement>> GetUserAchievementsAsync(int userId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT a.Id,
                   a.Title,
                   a.Description,
                   CASE WHEN ua.Id IS NOT NULL THEN 1 ELSE 0 END AS IsUnlocked
            FROM Achievements a
            LEFT JOIN UserAchievements ua
                   ON ua.AchievementId = a.Id AND ua.UserId = @UserId
            ORDER BY IsUnlocked DESC, a.Id", conn);

        cmd.Parameters.AddWithValue("@UserId", userId);

        var achievements = new List<Achievement>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            achievements.Add(new Achievement
            {
                AchievementId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                IsUnlocked = reader.GetInt32(3) == 1
            });
        }

        return achievements;
    }

    public async Task UnlockAchievementAsync(int userId, int achievementId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        var cmd = new SqlCommand(@"
            IF NOT EXISTS (SELECT 1 FROM UserAchievements WHERE UserId = @UserId AND AchievementId = @AchievementId)
                INSERT INTO UserAchievements (UserId, AchievementId) VALUES (@UserId, @AchievementId);", conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@AchievementId", achievementId);
        await cmd.ExecuteNonQueryAsync();
    }

}
