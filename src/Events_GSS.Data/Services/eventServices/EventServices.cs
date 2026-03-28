using System;
using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories.eventRepository;

namespace Events_GSS.Data.Services.eventServices;

public class EventService: IEventService
{
	private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<List<EventEntity>> GetAllPublicActiveEventsAsync()
        => await _eventRepository.GetAllPublicActiveAsync();

    public async Task<EventEntity?> GetEventByIdAsync(int eventId)
        => await _eventRepository.GetByIdAsync(eventId);

    public async Task CreateEventAsync(EventEntity eventEntity)
       => await _eventRepository.AddAsync(eventEntity);

    public async Task UpdateEventAsync(EventEntity eventEntity)
        => await _eventRepository.UpdateAsync(eventEntity);

    public async Task DeleteEventAsync(int eventId)
        => await _eventRepository.DeleteAsync(eventId);
}
