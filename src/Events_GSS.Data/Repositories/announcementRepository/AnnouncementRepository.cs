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
namespace Events_GSS.Data.Repositories.announcementRepository;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public AnnouncementRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    public async Task<int> AddAsync(Announcement announcement)
    {
        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            try
            {
                await connection.OpenAsync();
                string query = @"
                    INSERT INTO Announcements (Message, Date, IsPinned, IsEdited, EventId, UserId)
                    OUTPUT INSERTED.AnnId
                    VALUES (@Message, @Date, @IsPinned, @IsEdited, @EventId, @UserId)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Message", announcement.Message);
                    command.Parameters.AddWithValue("@Date", announcement.Date);
                    command.Parameters.AddWithValue("@IsPinned", announcement.IsPinned);
                    command.Parameters.AddWithValue("@IsEdited", announcement.IsEdited);
                    //command.Parameters.AddWithValue("@EventId", announcement.Event.Id);
                    //command.Parameters.AddWithValue("@UserId", announcement.Author.Id);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
            catch (SqlException se)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.Error.WriteLine($"SQL Exception: {se.Message}");
                // Optionally, rethrow the exception or handle it as needed
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while adding the announcement.", ex);
            }
        }
    }

    public async Task AddReactionAsync(int announcementId, int userId, string emoji)
    {
        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            try
            {
                await connection.OpenAsync();
                string query = @"
                    IF EXISTS (SELECT 1 FROM AnnouncementReactions WHERE AnnouncementId = @AnnouncementId AND UserId = @UserId)
                BEGIN
                    UPDATE AnnouncementReactions
                    SET Emoji = @Emoji
                    WHERE AnnouncementId = @AnnouncementId AND UserId = @UserId
                END
                ELSE
                BEGIN
                    INSERT INTO AnnouncementReactions (AnnouncementId, UserId, Emoji)
                    VALUES (@AnnouncementId, @UserId, @Emoji)
                END";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AnnouncementId", announcementId);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Emoji", emoji);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException se)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.Error.WriteLine($"SQL Exception: {se.Message}");
                // Optionally, rethrow the exception or handle it as needed
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while adding the reaction.", ex);
            }
        }
    }

    public async Task DeleteAsync(int announcementId)
    {
        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            try
            {
                await connection.OpenAsync();
                string query = @"
                        DELETE FROM Announcements 
                        WHERE AnnId = @AnnId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AnnId", announcementId);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException se)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.Error.WriteLine($"SQL Exception: {se.Message}");
                // Optionally, rethrow the exception or handle it as needed
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while deleting the announcement.", ex);
            }
        }
    }

    public async Task<List<Announcement>> GetByEventAsync(int eventId, int userId)
    {
        var announcements = new List<Announcement>();

        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            try
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
                        while (await reader.ReadAsync())
                        {
                            var announcement = new Announcement(
                                id: reader.GetInt32(reader.GetOrdinal("AnnId")),
                                message: reader.GetString(reader.GetOrdinal("Message")),
                                date: reader.GetDateTime(reader.GetOrdinal("Date")))
                            {
                                IsPinned = reader.GetBoolean(reader.GetOrdinal("IsPinned")),
                                IsEdited = reader.GetBoolean(reader.GetOrdinal("IsEdited")),
                                IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                                Event = new Event
                                {
                                    EventId = reader.GetInt32(reader.GetOrdinal("EventId")),
                                    Name = reader.GetString(reader.GetOrdinal("EventName"))
                                },
                                Author = new User
                                {
                                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                    Name = reader.GetString(reader.GetOrdinal("UserName"))
                                }
                            };

                            announcements.Add(announcement);
                        }
                    }
                }
            }catch(SqlException se)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.Error.WriteLine($"SQL Exception: {se.Message}");
                // Optionally, rethrow the exception or handle it as needed
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving announcements.", ex);
            }
        }

        return announcements;
    }

    public async Task<List<ReactionCounter>> GetReactionsAsync(int announcementId)
    {
        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            try
            {
                await connection.OpenAsync();

                string query = @"
                        SELECT Emoji, COUNT(*) as ReactionCount
                    FROM AnnouncementReactions
                    WHERE AnnouncementId = @AnnouncementId
                    GROUP BY Emoji";

                using (SqlCommand command = new SqlCommand(query, connection)) 
                { 
                    command.Parameters.AddWithValue("@AnnouncementId", announcementId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        var reactions = new List<ReactionCounter>();
                        while (await reader.ReadAsync())
                        {
                            reactions.Add(new ReactionCounter 
                            {
                                Emoji = reader.GetString(reader.GetOrdinal("Emoji")),
                                Count = reader.GetInt32(reader.GetOrdinal("ReactionCount"))
                            });

                        }
                        return reactions;
                    }
                }

            }
            catch (SqlException se)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.Error.WriteLine($"SQL Exception: {se.Message}");
                // Optionally, rethrow the exception or handle it as needed
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving the reactions.", ex);
            }
        }
    }

    public async Task<List<AnnouncementReadReceipt>> GetReadReceiptsAsync(int announcementId)
    {
        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            try
            {
                await connection.OpenAsync();

                string query = @"
                   SELECT u.Id as UserId, u.Name as UserName, r.ReadAt
                   FROM AnnouncementReadReceipts as r
                   INNER JOIN Users u ON r.UserId = u.Id
                   WHERE r.AnnouncementId = @AnnouncementId
                   ORDER BY r.ReadAt ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AnnouncementId", announcementId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        var receipts = new List<AnnouncementReadReceipt>();
                        while (await reader.ReadAsync())
                        {
                            receipts.Add(new AnnouncementReadReceipt
                            {
                                User = new User
                                {
                                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                    Name = reader.GetString(reader.GetOrdinal("UserName"))
                                },
                                ReadAt = reader.GetDateTime(reader.GetOrdinal("ReadAt"))
                            });
                        }
                        return receipts;
                    }
                }
            }
            catch (SqlException se)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.Error.WriteLine($"SQL Exception: {se.Message}");
                // Optionally, rethrow the exception or handle it as needed
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving the read receipts.", ex);
            }
        }
    }

    public async Task MarkAsReadAsync(int announcementId, int userId)
    {
        using( SqlConnection connection = _connectionFactory.CreateConnection())
        {
            try
            {
                await connection.OpenAsync();
                string query = @"
                    IF NOT EXISTS (SELECT 1 FROM AnnouncementReadReceipts WHERE AnnouncementId = @AnnouncementId AND UserId = @UserId)
                    BEGIN
                        INSERT INTO AnnouncementReadReceipts (AnnouncementId, UserId, ReadAt)
                        VALUES (@AnnouncementId, @UserId, GETUTCDATE())
                    END";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AnnouncementId", announcementId);
                    command.Parameters.AddWithValue("@UserId", userId);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException se)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.Error.WriteLine($"SQL Exception: {se.Message}");
                // Optionally, rethrow the exception or handle it as needed
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while marking the announcement as read.", ex);
            }
        }
    }

    public async Task PinAsync(int announcementId, int eventId)
    {
        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            try
            {
                await connection.OpenAsync();
                string query = @"
                    UPDATE Announcements
                    SET IsPinned = 1
                    WHERE AnnId = @AnnId AND EventId = @EventId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AnnId", announcementId);
                    command.Parameters.AddWithValue("@EventId", eventId);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException se)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.Error.WriteLine($"SQL Exception: {se.Message}");
                // Optionally, rethrow the exception or handle it as needed
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while pinning the announcement.", ex);
            }
        }
    }

    public async Task RemoveReactionAsync(int announcementId, int userId)
    {
        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            try
            {
                await connection.OpenAsync();
                string query = @"
                    DELETE FROM AnnouncementReactions 
                    WHERE AnnouncementId = @AnnouncementId AND UserId = @UserId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AnnouncementId", announcementId);
                    command.Parameters.AddWithValue("@UserId", userId);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException se)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.Error.WriteLine($"SQL Exception: {se.Message}");
                // Optionally, rethrow the exception or handle it as needed
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while removing the reaction.", ex);
            }
        }
    }

    public async Task UnpinAsync(int eventId)
    {
        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            try
            {
                await connection.OpenAsync();

                string query = @"
                   UPDATE Announcements
                    SET IsPinned = 0
                    WHERE EventId = @EventId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EventId", eventId);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException se)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.Error.WriteLine($"SQL Exception: {se.Message}");
                // Optionally, rethrow the exception or handle it as needed
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while unpinning announcements.", ex);
            }
        }
    }

    public async Task UpdateAsync(Announcement announcement)
    {
        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            try
            {
                await connection.OpenAsync();

                string query = @"
                   UPDATE Announcements
                    SET Message = @Message, IsEdited = 1
                    WHERE AnnId = @AnnId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Message", announcement.Message);
                    command.Parameters.AddWithValue("@AnnId", announcement.Id);

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException sx)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.Error.WriteLine($"SQL Exception: {sx.Message}");
                // Optionally, rethrow the exception or handle it as needed
                throw;

            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while updating the announcement.", ex);
            }
        }
    }
}
