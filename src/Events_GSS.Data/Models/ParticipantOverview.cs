using System;
using System.Collections.Generic;
using System.Text;

namespace Events_GSS.Data.Models;

public class ParticipantOverview
{
    public int TotalParticipants { get; set; }
    public int ActiveParticipants { get; set; }
    public double EngagementRate { get; set; }
}
