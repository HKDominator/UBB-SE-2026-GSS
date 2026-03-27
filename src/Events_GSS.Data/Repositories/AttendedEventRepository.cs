using System;
using System.Collections.Generic;
using System.Text;

using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories;

namespace Events_GSS.Data.Repositories
{
    public class AttendedEventRepository : IAttendedEventRepository
    {
        // sqlConnectionFactory
        public async Task AddAsync(AttendedEvent attendedEvent)
        {

        }

        public async Task DeleteAsync(int eventId, int userId)
        {

        }

        public async Task UpdateIsArchivedAsync(int eventId, int userId, bool isArchived)
        {

        }
        public async Task UpdateIsFavouriteAsync(int eventId, int userId, bool isFavourite)
        {

        }
        public async Task<AttendedEvent> GetAsync(int eventId, int userId)
        {
            return null;
        }
        public async Task<List<AttendedEvent>> GetByUserIdAsync(int userId)
        {
            return null;
        }
    }
}
