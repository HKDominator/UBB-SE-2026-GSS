using System;
using System.Collections.Generic;
using System.Text;

namespace Events_GSS.Data.Models;

public class LeaderboardEntry
{
    public User Participant { get; set; }
    public int MessagesCount { get; set; }
    public int MemoriesCount { get; set; }
    public int QuestsCompleted { get; set; }
    public int TotalScore { get; set; }
}
