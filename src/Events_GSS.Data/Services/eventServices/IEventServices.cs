using System;
using Events_GSS.Data.Models;

namespace Events_GSS.Data.Services.eventServices;

public interface IEventService
{
    Task<List<EventEntity>> GetAllPublicActiveEventsAsync();
    Task<EventEntity?> GetEventByIdAsync(int eventId);
    Task CreateEventAsync(EventEntity eventEntity);
    Task UpdateEventAsync(EventEntity eventEntity);
    Task DeleteEventAsync(int eventId);
}
