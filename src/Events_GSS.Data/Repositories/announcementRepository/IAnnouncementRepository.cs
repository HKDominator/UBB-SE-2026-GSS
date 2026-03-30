using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Events_GSS.Data.Models;

namespace Events_GSS.Data.Repositories.Interfaces;

public interface IAnnouncementRepository
{
    Task<List<Announcement>> GetByEventAsync(int eventId, int userId);
    Task<int> AddAsync(Announcement announcement);
    Task UpdateAsync(Announcement announcement);
    Task DeleteAsync(int announcementId);
    Task PinAsync(int announcementId, int eventId);
    Task UnpinAsync(int eventId);
    Task MarkAsReadAsync(int announcementId, int userId);
    Task<List<AnnouncementReadReceipt>> GetReadReceiptsAsync(int announcementId);
    Task AddReactionAsync(int announcementId, int userId, string emoji);
    Task RemoveReactionAsync(int announcementId, int userId);
    Task<List<ReactionCounter>> GetReactionsAsync(int announcementId);


}
