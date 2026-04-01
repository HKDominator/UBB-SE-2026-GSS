using System.Diagnostics;

using Events_GSS.Data.Database;
using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories;
using Events_GSS.Data.Repositories.Interfaces;
using Events_GSS.Data.Services;
using Events_GSS.Data.Services.Interfaces;
using Events_GSS.Services.Interfaces;
using Events_GSS.ViewModels;
using Events_GSS.Views;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.WindowsAppSDK.Runtime.Packages;

namespace Events_GSS;

public sealed partial class MainWindow : Window
{
 //   public QuestAdminViewModel QuestViewModel { get; }
    //public MemoryViewModel MemoriesViewModel { get; }
    public MainWindow()
    {
        InitializeComponent();

        IQuestService qs = App.Services.GetRequiredService<IQuestService>();
     //   IMemoryService memoryService = App.Services.GetRequiredService<IMemoryService>();
        IQuestApprovalService questApprovalService= App.Services.GetRequiredService<IQuestApprovalService>();
        
        IUserService userService =App.Services.GetRequiredService<IUserService>();
        // TODO: PlaceHolder for event data, replace when navigation is implemented
        var currentUser = new User { UserId = 1 };
        var currentUser2 = new User { UserId = 2 };
        var currentEvent = new Event { EventId = 1, Admin = currentUser };

        var qAdminVM = new QuestAdminViewModel(currentEvent, qs);

        //var vm = new QuestUserViewModel(currentEvent, questApprovalService, userService);
        //QuestUserPage.ViewModel = vm;
        var wholeAdminApprovalVm = new QuestApprovalViewModel(qAdminVM, questApprovalService);
        //   MemoriesViewModel = new MemoryViewModel(memoryService);
        QuestApprovalPage.ViewModel = wholeAdminApprovalVm;
        this.Activated += async (s, e) =>
        {
            //await MemoriesView.LoadAsync(currentEvent, currentUser);
            

        };
        
    }
}