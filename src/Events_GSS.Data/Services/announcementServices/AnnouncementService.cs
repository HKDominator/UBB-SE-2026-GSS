using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories;
using Events_GSS.Data.Repositories.announcementRepository; 
using Events_GSS.Data.Repositories.eventRepository;


namespace Events_GSS.Data.Services.announcementServices;

public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IEventRepository _eventRepository;

    public AnnouncementService(IAnnouncementRepository announcementRepository)
    {
        _announcementRepository = announcementRepository;
    }

    public async Task CreateAnnouncementAsync(string message, int eventId, int userId)
    {
        var eventEntity = await _announcementRepository.GetEventById(eventId);
        var userEntity = await _announcementRepository.GetUserById(userId);
        if( eventEntity == null)
        {
            throw new ArgumentException($"Event with ID {eventId} does not exist.");
        }
        if( userEntity == null)
        {
            throw new ArgumentException($"User with ID {userId} does not exist.");
        }

        var announcement = new Announcement(
            id: 0, // Id will be set by the database
            message: message,
            date: DateTime.UtcNow)
        {
            IsPinned = false,
            IsEdited = false,
            Event = eventEntity,
            Author = userEntity
        };
        await _announcementRepository.AddAsync(announcement);

    }

    public async Task DeleteAnnouncementAsync(int annId, int userId)
    { //TO DO : check if user is author or event admin, check if the event is existing
        await _announcementRepository.DeleteAsync(annId, userId);
    }

    public async Task<List<Announcement>> GetAnnouncementsAsync(int eventId, int userId)
    {
        return await _announcementRepository.GetByEventAsync(eventId, userId);
    }

    public Task<List<AnnouncementReadReceipt>> GetReadReceiptsAsync(int annId, int userId)
    {
        return _announcementRepository.GetReadReceiptsAsync(annId);
    }

    public async Task MarkAsReadAsync(int annId, int userId)
    {
        await _announcementRepository.MarkAsReadAsync(annId, userId);
    }

    public async Task PinAnnouncementAsync(int annId, int eventId, int userId)
    {
        await _announcementRepository.UnpinAsync(eventId);
        await _announcementRepository.PinAsync(annId, eventId);
    }

    public async Task ReactAsync(int annId, int userId, string emoji)
    {
        await _announcementRepository.AddReactionAsync(annId, userId, emoji);
    }

    public async Task RemoveReactionAsync(int annId, int userId)
    {
        await _announcementRepository.RemoveReactionAsync(annId, userId);
    }

    public async Task UpdateAnnouncementAsync(int announcementId, string newMessage, int userId)
    {
        if(string.IsNullOrEmpty(newMessage))
        {
            throw new ArgumentException("Message cannot be null or empty.", nameof(newMessage));
        }
        var newAnn = new Announcement(announcementId, newMessage, DateTime.UtcNow)
        {
            IsEdited = true
        };
        await _announcementRepository.UpdateAsync(newAnn);
    }
}
