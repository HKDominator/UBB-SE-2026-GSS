using System;
using System.Collections.Generic;
using System.Text;

using Events_GSS.Data.Models;
using Events_GSS.Data.Services.Interfaces;

namespace Events_GSS.ViewModels;

public class QuestUserViewModel
{
    private readonly IQuestApprovalService _approvalService;
    private readonly Event _event;
    
    public QuestUserViewModel(IQuestApprovalService approvalService)
    {
        _approvalService = approvalService;
    }



}
