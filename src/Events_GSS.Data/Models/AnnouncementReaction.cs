using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Identity.Client;

namespace Events_GSS.Data.Models;

public class AnnouncementReaction

{
    public int Id { get; set; }
    public required string Emoji { get; set; }
    public required Announcement Announcement { get; set; }

    public AnnouncementReaction(int id, string emoji, Announcement announcement)
    {
        Id = id;
        Emoji = emoji;
        Announcement = announcement;
    }

    //public User Author {  get; set; }
}
