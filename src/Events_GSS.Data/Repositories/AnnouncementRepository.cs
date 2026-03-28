using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Events_GSS.Data.Database;
using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories.Interfaces;

using Microsoft.Data.SqlClient;

using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Events_GSS.Data.Repositories;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public AnnouncementRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    public Task<int> AddAsync(Announcement announcement)
    {
        throw new NotImplementedException();
    }

    public Task AddReactionAsync(int announcementId, int userId, string emoji)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int announcementId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Announcement>> GetByEventAsync(int eventId, int userId)
    {
        var announcements = new List<Announcement>();

        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            await connection.OpenAsync();

            string query = @"
            SELECT 
                a.AnnId, a.Message, a.Date, a.IsPinned, a.IsEdited,
                e.EventId, e.Name AS EventName, -- Adjust column names if needed
                u.Id, u.Name AS UserName,   -- Adjust column names if needed
                CAST(CASE WHEN r.UserId IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsRead
            FROM Announcements a
            INNER JOIN Events e ON a.EventId = e.EventId
            INNER JOIN Users u ON a.UserId = u.UserId
            LEFT JOIN AnnouncementReadReceipts r ON a.AnnId = r.AnnouncementId AND r.UserId = @UserId
            WHERE a.EventId = @EventId
            ORDER BY a.IsPinned DESC, a.Date DESC";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EventId", eventId);
                command.Parameters.AddWithValue("@UserId", userId);
                using (SqlDataReader reader = await command.ExecuteReaderAsync()) 
                {
                    while( await reader.ReadAsync())
                    {
                        var announcement = new Announcement(
                            id : reader.GetInt32(reader.GetOrdinal("AnnId")),
                            message : reader.GetString(reader.GetOrdinal("Message")),
                            date : reader.GetDateTime(reader.GetOrdinal("Date")))
                        { 
                            IsPinned = reader.GetBoolean(reader.GetOrdinal("IsPinned")),
                            IsEdited = reader.GetBoolean(reader.GetOrdinal("IsEdited")),
                            IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                            //Event = new Event
                            //{
                            //    Id = reader.GetInt32(reader.GetOrdinal("EventId")),
                            //    Name = reader.GetString(reader.GetOrdinal("EventName"))
                            //},
                            //Author = new Author
                            //{
                            //    Id = reader.GetInt32(reader.GetOrdinal("UserId")),
                            //    Name = reader.GetString(reader.GetOrdinal("UserName"))
                            //}
                        };

                        announcements.Add(announcement);
                    }
                }
            }
        }

        return announcements;
    }

    public Task<List<AnnouncementReaction>> GetReactionsAsync(int announcementId)
    {
        throw new NotImplementedException();
    }

    public Task<List<int>> GetReadReceiptsAsync(int announcementId)
    {
        throw new NotImplementedException();
    }

    public Task MarkAsReadAsync(int announcementId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task PinAsync(int announcementId, int eventId)
    {
        throw new NotImplementedException();
    }

    public Task RemoveReactionAsync(int announcementId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task UnpinAsync(int eventId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Announcement announcement)
    {
        throw new NotImplementedException();
    }
}
