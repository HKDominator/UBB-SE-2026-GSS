using System;
using System.Collections.Generic;
using System.Text;

namespace Events_GSS.Data.Models;

public class EngagementBreakdown
{
    public int TotalMessages { get; set; }
    public int TotalMemories { get; set; }
    public int TotalQuestSubmissions { get; set; }
    public double ApprovedQuestsRate { get; set; }
    public double DeniedQuestsRate { get; set; }
}
