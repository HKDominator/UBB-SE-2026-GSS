using System;
using System.Collections.Generic;
using System.Text;

namespace Events_GSS.Data.Models
{
    public class AttendedEvent
    {
        private Event Event { get; set; }
        private User User { get; set; }
        private DateTime EnrollmentDate { get; set; }
        private Boolean IsArchived { get; set; }
        private Boolean IsFavourite {  get; set; }

        public AttendedEvent(Event @event, User user, DateTime enrollmentDate, bool isArchived, bool isFavourite)
        {
            Event = @event;
            User = user;
            EnrollmentDate = enrollmentDate;
            IsArchived = isArchived;
            IsFavourite = isFavourite;
        }
    }
}
