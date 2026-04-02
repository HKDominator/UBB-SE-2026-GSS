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
    private Event? _event;
    private int _currentUserId;
    private bool _isAttending;
    private IAttendedEventService? _attendedService;

    public EventDetailPage()
    {
        InitializeComponent();
    }

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        _nav = App.Services.GetRequiredService<INavigationService>();

        if (e.Parameter is not Event ev) return;

        _event = ev;

        EventNameText.Text = ev.Name;
        EventInfoText.Text = $"{ev.StartDateTime:MMM dd, yyyy HH:mm} • {ev.Location}";

        EventDateRangeText.Text = $"{ev.StartDateTime:MMM d, yyyy h:mm tt} → {ev.EndDateTime:MMM d, yyyy h:mm tt}";
        DescriptionText.Text = ev.Description ?? string.Empty;

        ParticipantsText.Text = $"Participants: {ev.EnrolledCount} / {(ev.MaximumPeople?.ToString() ?? "—")}";

        var userService = App.Services.GetRequiredService<IUserService>();
        var currentUser = userService.GetCurrentUser();
        int userId = currentUser.UserId;
        _currentUserId = userId;
        _attendedService = App.Services.GetService<IAttendedEventService>();
        if (_attendedService != null)
        {
            var existing = await _attendedService.GetAsync(ev.EventId, userId);
            _isAttending = existing != null;
        }

        UpdateJoinButton();
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

    private void UpdateJoinButton()
    {
        if (_event == null) return;

        JoinButton.Content = _isAttending ? "Not attend event anymore" : "Join Event";
        JoinButton.IsEnabled = _event.MaximumPeople == null || _event.EnrolledCount < _event.MaximumPeople;
    }

    private async void OnJoinLeaveClicked(object sender, RoutedEventArgs e)
    {
        if (_event == null || _attendedService == null) return;

        if (_isAttending)
        {
            await _attendedService.LeaveEventAsync(_event.EventId, _currentUserId);
            _event.EnrolledCount = Math.Max(0, _event.EnrolledCount - 1);
            _isAttending = false;
        }
        else
        {
            await _attendedService.AttendEventAsync(_event.EventId, _currentUserId);
            _event.EnrolledCount += 1;
            _isAttending = true;
        }

        // Update UI on UI thread
        _ = DispatcherQueue.TryEnqueue(() => {
            ParticipantsText.Text = $"Participants: {_event.EnrolledCount} / {(_event.MaximumPeople?.ToString() ?? "—")}";
            UpdateJoinButton();
        });
    }
}
