using Events_GSS.Data.Models;
using Events_GSS.Data.Services.announcementServices;
using Events_GSS.Data.Services.discussionService;
using Events_GSS.Data.Services.Interfaces;
using Events_GSS.Services;
using Events_GSS.Services.Interfaces;
using Events_GSS.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Events_GSS.Views;

public sealed partial class EventDetailPage : Page
{
    private INavigationService? _nav;
    private Event? _currentEvent;

    public EventDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        _nav = App.Services.GetRequiredService<INavigationService>();

        if (e.Parameter is not Event ev) return;

        _currentEvent = ev;
        EventNameText.Text = ev.Name;
        EventInfoText.Text = $"{ev.StartDateTime:MMM dd, yyyy HH:mm} • {ev.Location}";

        var userService = App.Services.GetRequiredService<IUserService>();
        var currentUser = userService.GetCurrentUser();
        int userId = currentUser.UserId;
        bool isAdmin = ev.Admin?.UserId == userId;

        var annService = App.Services.GetRequiredService<IAnnouncementService>();
        var annVm = new AnnouncementViewModel(ev, annService, userId, isAdmin);
        AnnouncementTab.ViewModel = annVm;
        _ = annVm.InitializeAsync();

        var discService = App.Services.GetRequiredService<IDiscussionService>();
        var discVm = new DiscussionViewModel(ev, discService, userId, isAdmin);
        DiscussionTab.ViewModel = discVm;
        _ = discVm.InitializeAsync();

        
        QuestAdminTab.ViewModel = new QuestApprovalViewModel(new QuestAdminViewModel(ev));
        QuestUserTab.ViewModel = new QuestUserViewModel(ev);
        if(isAdmin)
        {
            QuestAdminTab.Visibility = Visibility.Visible;
            QuestUserTab.Visibility = Visibility.Collapsed;
            StatsButton.Visibility = Visibility.Visible;
        }
        else
        {
            StatsButton.Visibility = Visibility.Collapsed;
        }

        var memService = App.Services.GetRequiredService<IMemoryService>();
        var memVm = new MemoryViewModel(memService);
        MemoryTab.ViewModel = memVm;
        _ = memVm.InitializeAsync(ev, currentUser);
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)
    {
        _nav?.GoBack();
    }

    private void OnStatsClicked(object sender, RoutedEventArgs e)
    {
        if (_currentEvent is null) return;
        _nav?.NavigateTo(PageKeys.Statistics, _currentEvent);
    }
}
