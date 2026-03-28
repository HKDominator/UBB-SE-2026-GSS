using System;
namespace Events_GSS.Data.Repositories.eventRepository;

public interface IEventRepository
{
    Task<List<EventEntity>> GetAllPublicActiveAsync();
    Task<EventEntity?> GetByIdAsync(int eventId);
    Task AddAsync(EventEntity eventEntity);
    Task UpdateAsync(EventEntity eventEntity);
    Task DeleteAsync(int eventId);
};