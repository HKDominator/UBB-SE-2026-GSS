using Events_GSS.Data.Database;
using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories;
using Events_GSS.Data.Services.Interfaces;
using Events_GSS.Data.Services; 
using Events_GSS.ViewModels;
using Events_GSS.Data.Services.discussionService;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.WindowsAppSDK.Runtime.Packages;

namespace Events_GSS;

public sealed partial class MainWindow : Window
{
    public QuestAdminViewModel QuestViewModel { get; }
    public DiscussionViewModel DiscussionViewModel { get; }
    public MemoryViewModel MemoriesViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();

        var services = App.Services.GetRequiredService<IDiscussionService>();
        IQuestService qs = App.Services.GetRequiredService<IQuestService>();
        IMemoryService memoryService = App.Services.GetRequiredService<IMemoryService>();

        // TODO: PlaceHolder for event data, replace when navigation is implemented
        var currentEvent = new Event { EventId = 1 };
        var currentUserId = 1; // Placeholder for current user ID, replace with actual user context
        bool isAdmin = true; // Placeholder for admin check, replace with actual logic
        var currentUser = new User { UserId = 1 };
        var currentUser2 = new User { UserId = 2 };
        var currentEvent = new Event { EventId = 1, Admin = currentUser };

        DiscussionViewModel = new DiscussionViewModel(currentEvent, services, currentUserId, isAdmin);
        QuestViewModel = new QuestAdminViewModel(currentEvent, qs);
        MemoriesViewModel = new MemoryViewModel(memoryService);
        this.Activated += async (s, e) =>
        {
            await MemoriesView.LoadAsync(currentEvent, currentUser);
        };
        _ = DiscussionViewModel.InitializeAsync();
    }
}