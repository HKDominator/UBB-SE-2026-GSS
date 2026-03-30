using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using Events_GSS.Data.Models;
using Events_GSS.Data.Services.announcementServices;

namespace Events_GSS.ViewModels;

// ── Payload for reaction commands ────────────────────────────────────────────
public record ReactionPayload(AnnouncementItemViewModel Announcement, string Emoji);

public partial class AnnouncementViewModel : ObservableObject
{
    private readonly IAnnouncementService _announcementService;
    private readonly Event _event;
    private readonly int _currentUserId;

    public AnnouncementViewModel(
        Event forEvent,
        IAnnouncementService announcementService,
        int currentUserId,
        bool isAdmin)
    {
        _event = forEvent;
        _announcementService = announcementService;
        _currentUserId = currentUserId;
        IsEventAdmin = isAdmin;

        Announcements = new ObservableCollection<AnnouncementItemViewModel>();
    }

    // ── Collections ──────────────────────────────────────────────────────────

    public ObservableCollection<AnnouncementItemViewModel> Announcements { get; }

    // ── Observable state ─────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEventAdmin;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ErrorVisibility))]
    private string? _errorMessage;

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private string _newMessage = string.Empty;

    [ObservableProperty]
    private AnnouncementItemViewModel? _selectedAnnouncement;

    // ── Computed ─────────────────────────────────────────────────────────────

    public bool IsNotLoading => !IsLoading;
    public bool HasError => ErrorMessage is not null;
    public Visibility ErrorVisibility => HasError ? Visibility.Visible : Visibility.Collapsed;

    // ── Initialization ───────────────────────────────────────────────────────
    //
    //  Explicit method instead of fire-and-forget in the constructor.
    //  The page calls this in OnNavigatedTo so the caller owns the Task
    //  and exceptions don't vanish silently.

    public async Task InitializeAsync()
    {
        await RunGuardedAsync(LoadAnnouncementsAsync);
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadAnnouncementsAsync()
    {
        var list = await _announcementService.GetAnnouncementsAsync(
            _event.EventId, _currentUserId);

        Announcements.Clear();
        foreach (var a in list)
        {
            Announcements.Add(new AnnouncementItemViewModel(a));
        }

        UpdateUnreadCount();
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateAnnouncementAsync()
    {
        await RunGuardedAsync(async () =>
        {
            await _announcementService.CreateAnnouncementAsync(
                NewMessage.Trim(), _event.EventId, _currentUserId);

            NewMessage = string.Empty;
            await LoadAnnouncementsAsync();
        });
    }

    private bool CanCreate() =>
        !string.IsNullOrWhiteSpace(NewMessage) && IsNotLoading;

    [RelayCommand(CanExecute = nameof(CanModify))]
    private async Task UpdateAnnouncementAsync()
    {
        await RunGuardedAsync(async () =>
        {
            await _announcementService.UpdateAnnouncementAsync(
                SelectedAnnouncement!.Id, NewMessage.Trim(), _currentUserId);

            NewMessage = string.Empty;
            SelectedAnnouncement = null;
            await LoadAnnouncementsAsync();
        });
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    private async Task DeleteAnnouncementAsync()
    {
        await RunGuardedAsync(async () =>
        {
            await _announcementService.DeleteAnnouncementAsync(
                SelectedAnnouncement!.Id, _currentUserId);

            Announcements.Remove(SelectedAnnouncement);
            SelectedAnnouncement = null;
            UpdateUnreadCount();
        });
    }

    private bool CanModify() =>
        SelectedAnnouncement is not null && IsNotLoading;

    [RelayCommand(CanExecute = nameof(CanPin))]
    private async Task PinAnnouncementAsync()
    {
        await RunGuardedAsync(async () =>
        {
            await _announcementService.PinAnnouncementAsync(
                SelectedAnnouncement!.Id, _event.EventId, _currentUserId);

            await LoadAnnouncementsAsync();
        });
    }

    // Only admins can pin, and something must be selected
    private bool CanPin() =>
        SelectedAnnouncement is not null && IsNotLoading && IsEventAdmin;

    [RelayCommand]
    private async Task ToggleExpandAsync(AnnouncementItemViewModel? item)
    {
        if (item is null) return;

        item.IsExpanded = !item.IsExpanded;

        if (item.IsExpanded && !item.IsRead)
        {
            await RunGuardedAsync(async () =>
            {
                await _announcementService.MarkAsReadAsync(item.Id, _currentUserId);
                item.IsRead = true;
                UpdateUnreadCount();
            });
        }
    }

    // ── Reactions ────────────────────────────────────────────────────────────
    //
    //  Single-object parameter so the source-generated IAsyncRelayCommand<T>
    //  is easy to bind from XAML or call from code-behind.

    [RelayCommand]
    private async Task AddReactionAsync(ReactionPayload? payload)
    {
        if (payload is null) return;

        await RunGuardedAsync(async () =>
        {
            await _announcementService.ReactAsync(
                payload.Announcement.Id, _currentUserId, payload.Emoji);

            await LoadAnnouncementsAsync();
        });
    }

    [RelayCommand]
    private async Task RemoveReactionAsync(AnnouncementItemViewModel? item)
    {
        if (item is null) return;

        await RunGuardedAsync(async () =>
        {
            await _announcementService.RemoveReactionAsync(item.Id, _currentUserId);

            await LoadAnnouncementsAsync();
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void UpdateUnreadCount() =>
        UnreadCount = Announcements.Count(a => !a.IsRead);

    // Centralises the loading-flag + error-handling boilerplate so every
    // command body stays focused on its own logic.
    private async Task RunGuardedAsync(Func<Task> action)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await action();
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "You must join this event to view announcements.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Whenever IsLoading or SelectedAnnouncement change, poke every
    // command whose CanExecute depends on them.
    partial void OnIsLoadingChanged(bool value) => NotifyCommandsChanged();
    partial void OnSelectedAnnouncementChanged(AnnouncementItemViewModel? value) => NotifyCommandsChanged();
    partial void OnNewMessageChanged(string value) => NotifyCommandsChanged();

    private void NotifyCommandsChanged()
    {
        CreateAnnouncementCommand.NotifyCanExecuteChanged();
        UpdateAnnouncementCommand.NotifyCanExecuteChanged();
        DeleteAnnouncementCommand.NotifyCanExecuteChanged();
        PinAnnouncementCommand.NotifyCanExecuteChanged();
    }
}