using System;
using System.Collections.Generic;
using System.Text;

using Events_GSS.Data.Models;

namespace Events_GSS.Data.Services.announcementServices;

public class AnnouncementService : IAnnouncementService
{
    public Task CreateAnnouncementAsync(string message, int eventId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAnnouncementAsync(int annId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Announcement>> GetAnnouncementsAsync(int eventId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<AnnouncementReadReceipt>> GetReadReceiptsAsync(int annId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task MarkAsReadAsync(int annId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task PinAnnouncementAsync(int annId, int eventId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task ReactAsync(int annId, int userId, string emoji)
    {
        throw new NotImplementedException();
    }

    public Task RemoveReactionAsync(int annId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAnnouncementAsync(int announcementId, string newMessage, int userId)
    {
        throw new NotImplementedException();
    }
}
