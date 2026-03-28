using System;
using Events_GSS.Data.Models;

namespace Events_GSS.Data.Repositories.eventRepository;

public interface IEventService
{
    Task<List<EventEntity>> GetAllPublicActiveEventsAsync();
    Task<EventEntity?> GetEventByIdAsync(int eventId);
    Task CreateEventAsync(EventEntity eventEntity);
    Task UpdateEventAsync(EventEntity eventEntity);
    Task DeleteEventAsync(int eventId);

    Task<List<EventEntity>> SearchByTitleAsync(string title);
    Task<List<EventEntity>> FilterByCategoryAsync(string category);
    Task<List<EventEntity>> FilterByLocationAsync(string location);
    Task<List<EventEntity>> FilterByDateAsync(DateTime date);
    Task<List<EventEntity>> FilterByDateRangeAsync(DateTime from, DateTime to);
}
