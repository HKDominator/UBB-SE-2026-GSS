using System;
using System.Data;
using Events_GSS.Data.Models;
using Microsoft.Data.SqlClient;
using Events_GSS.Data.Database.SqlConnectionFactory;

namespace Events_GSS.Data.Repositories.eventRepository;

public class EventRepository: IEventRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public EventRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<EventEntity>> GetAllPublicActiveAsync()
    {
        var events = new List<EventEntity>();
        using var connection=_connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"SELECT E.*, C.Title as CategoryTitle,
                                      u.UserId as UserId, u.Name as UserName,
                                      (SELECT COUNT(*) FROM AttendedEvents AE WHERE AE.EventId = E.EventId) AS EnrolledCount
                                    FROM EVENTS E
                                    LEFT JOIN CATEGORIES C ON E.CategoryId = C.CategoryId
                                    LEFT JOIN Users u ON E.CreatedBy = u.UserId
                                    WHERE E.IsPublic = 1 AND E.EndDateTime > GETUTCDATE()
                                    ORDER BY E.StartDateTime ASC", connection);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            events.Add(MapEvent(reader));
        }

        return events;
    }

    public async Task<EventEntity?> GetByIdAsync(int eventId)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT e.*, c.Title as CategoryTitle, 
            u.UserId as UserId, u.Name as UserName,
            (SELECT COUNT(*) FROM AttendedEvents ae WHERE ae.EventId = e.EventId) AS EnrolledCount
            FROM Events e
            LEFT JOIN Categories c ON e.CategoryId = c.CategoryId
            LEFT JOIN Users u ON e.CreatedBy = u.UserId
            WHERE e.EventId = @EventId", conn);
        cmd.Parameters.AddWithValue("@EventId", eventId);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapEvent(reader) : null;
    }

    public async Task AddAsync(EventEntity eventEntity)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            INSERT INTO Events 
                (Name, LocationLat, LocationLng, StartDateTime, EndDateTime, 
                 IsPublic, Description, MaximumPeople, EventBannerPath, CategoryId, CreatedBy)
            VALUES 
                (@Name, @Lat, @Lng, @Start, @End,
                 @IsPublic, @Desc, @MaxPeople, @Banner, @CategoryId, @CreatedBy)", conn);

        cmd.Parameters.AddWithValue("@Name", eventEntity.Name);
        cmd.Parameters.AddWithValue("@Lat", eventEntity.LocationLat);
        cmd.Parameters.AddWithValue("@Lng", eventEntity.LocationLng);
        cmd.Parameters.AddWithValue("@Start", eventEntity.StartDateTime);
        cmd.Parameters.AddWithValue("@End", eventEntity.EndDateTime);
        cmd.Parameters.AddWithValue("@IsPublic", eventEntity.IsPublic);
        cmd.Parameters.AddWithValue("@Desc", (object?)eventEntity.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MaxPeople", (object?)eventEntity.MaximumPeople ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Banner", (object?)eventEntity.EventBannerPath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CategoryId", (object?)eventEntity.CategoryId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedBy", eventEntity.CreatedBy?.UserId?? throw new ArgumentNullException("CreatedBy is required"));

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(EventEntity eventEntity)
        {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            UPDATE Events SET
                Name = @Name,
                LocationLat = @Lat,
                LocationLng = @Lng,
                StartDateTime = @Start,
                EndDateTime = @End,
                IsPublic = @IsPublic,
                Description = @Desc,
                MaximumPeople = @MaxPeople,
                EventBannerPath = @Banner,
                CategoryId = @CategoryId
            WHERE EventId = @EventId", conn);

        cmd.Parameters.AddWithValue("@EventId", eventEntity.EventId);
        cmd.Parameters.AddWithValue("@Name", eventEntity.Name);
        cmd.Parameters.AddWithValue("@Lat", eventEntity.LocationLat);
        cmd.Parameters.AddWithValue("@Lng", eventEntity.LocationLng);
        cmd.Parameters.AddWithValue("@Start", eventEntity.StartDateTime);
        cmd.Parameters.AddWithValue("@End", eventEntity.EndDateTime);
        cmd.Parameters.AddWithValue("@IsPublic", eventEntity.IsPublic);
        cmd.Parameters.AddWithValue("@Desc", (object?)eventEntity.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MaxPeople", (object?)eventEntity.MaximumPeople ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Banner", (object?)eventEntity.EventBannerPath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CategoryId", (object?)eventEntity.CategoryId ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int eventId)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();

        var cmd = new SqlCommand("DELETE FROM Events WHERE EventId = @EventId", conn);
        cmd.Parameters.AddWithValue("@EventId", eventId);

        await cmd.ExecuteNonQueryAsync();
    }

    private static EventEntity MapEvent(SqlDataReader reader) => new()
    {
        EventId = reader.GetInt32("EventId"),
        Name = reader.GetString("Name"),
        LocationLat = reader.GetDouble("LocationLat"),
        LocationLng = reader.GetDouble("LocationLng"),
        StartDateTime = reader.GetDateTime("StartDateTime"),
        EndDateTime = reader.GetDateTime("EndDateTime"),
        IsPublic = reader.GetBoolean("IsPublic"),
        Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
        MaximumPeople = reader.IsDBNull("MaximumPeople") ? null : reader.GetInt32("MaximumPeople"),
        EventBannerPath = reader.IsDBNull("EventBannerPath") ? null : reader.GetString("EventBannerPath"),
        CategoryId = reader.IsDBNull("CategoryId") ? null : reader.GetInt32("CategoryId"),
        CreatedBy=reader.IsDBNull("UserId") ? null : new User
        {
            UserId = reader.GetInt32("UserId"),
            Name = reader.GetString("UserName")
        },
        SlowModeSeconds = reader.IsDBNull("SlowModeSeconds") ? null : reader.GetInt32("SlowModeSeconds"),
        CategoryTitle = reader.IsDBNull("CategoryTitle") ? null : reader.GetString("CategoryTitle"),
        EnrolledCount = reader.GetInt32("EnrolledCount"),
    };
}
