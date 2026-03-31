using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Events_GSS.Data.Models;
using Events_GSS.Data.Services.discussionService;
using Events_GSS.Data.Services.Interfaces;

using Microsoft.UI.Xaml;

namespace Events_GSS.ViewModels;

// ── Payload records for commands with multiple parameters ────────────────────
public record DiscussionReactionPayload(DiscussionMessageItemViewModel Message, string Emoji);
public record MutePayload(int TargetUserId, DateTime? Until);

public partial class DiscussionViewModel : ObservableObject
{
    private readonly IDiscussionService _service;
    private readonly Event _event;
    private readonly int _currentUserId;

    public DiscussionViewModel(
        Event forEvent,
        IDiscussionService service,
        int currentUserId,
        bool isAdmin)
    {
        _event = forEvent;
        _service = service;
        _currentUserId = currentUserId;
        IsEventAdmin = isAdmin;

        Messages = new ObservableCollection<DiscussionMessageItemViewModel>();
    }

    // ── Collections ──────────────────────────────────────────────────────────

    public ObservableCollection<DiscussionMessageItemViewModel> Messages { get; }

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
    private string _newMessage = string.Empty;

    [ObservableProperty]
    private string? _mediaPath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInReplyMode))]
    private DiscussionMessageItemViewModel? _replyTarget;

    [ObservableProperty]
    private bool _isMuted;

    [ObservableProperty]
    private string? _muteRemainingText;

    [ObservableProperty]
    private int _slowModeRemainingSeconds;

    // ── Computed ─────────────────────────────────────────────────────────────

    public bool IsNotLoading => !IsLoading;
    public bool HasError => ErrorMessage is not null;
    public Visibility ErrorVisibility => HasError ? Visibility.Visible : Visibility.Collapsed;
    public bool IsInReplyMode => ReplyTarget is not null;

    // ── Initialization ───────────────────────────────────────────────────────
    //
    //  Called from the page's OnNavigatedTo so the caller owns the Task.

    public async Task InitializeAsync()
    {
        await RunGuardedAsync(LoadMessagesAsync);
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadMessagesAsync()
    {
        var list = await _service.GetMessagesAsync(
            _event.EventId, _currentUserId);

        Messages.Clear();
        foreach (var m in list)
        {
            Messages.Add(new DiscussionMessageItemViewModel(m));
        }
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendMessageAsync()
    {
        await RunGuardedAsync(async () =>
        {
            try
            {
                await _service.CreateMessageAsync(
                    NewMessage.Trim(),
                    MediaPath,
                    _event.EventId,
                    _currentUserId,
                    ReplyTarget?.Id);

                NewMessage = string.Empty;
                MediaPath = null;
                ReplyTarget = null;
                IsMuted = false;
                SlowModeRemainingSeconds = 0;

                await LoadMessagesAsync();
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains("muted", StringComparison.OrdinalIgnoreCase))
            {
                IsMuted = true;
                MuteRemainingText = ex.Message;
                throw; // let RunGuardedAsync set ErrorMessage too
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains("Slow mode", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(ex.Message, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int secs))
                {
                    SlowModeRemainingSeconds = secs;
                }

                throw;
            }
        });
    }

    private bool CanSend() =>
        (!string.IsNullOrWhiteSpace(NewMessage) || !string.IsNullOrWhiteSpace(MediaPath))
        && IsNotLoading
        && !IsMuted;

    [RelayCommand]
    private async Task DeleteMessageAsync(DiscussionMessageItemViewModel? item)
    {
        if (item is null) return;

        await RunGuardedAsync(async () =>
        {
            await _service.DeleteMessageAsync(
                item.Id, _currentUserId, _event.EventId);

            Messages.Remove(item);

            // Mark replies whose original was just deleted
            foreach (var m in Messages)
            {
                if (m.ReplyTo?.Id == item.Id)
                {
                    m.IsOriginalDeleted = true;
                }
            }
        });
    }

    // ── Reply ────────────────────────────────────────────────────────────────

    [RelayCommand]
    private void SetReplyTarget(DiscussionMessageItemViewModel? item)
    {
        ReplyTarget = item;
    }

    [RelayCommand]
    private void CancelReply()
    {
        ReplyTarget = null;
    }

    // ── Reactions ────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task AddReactionAsync(DiscussionReactionPayload? payload)
    {
        if (payload is null) return;

        await RunGuardedAsync(async () =>
        {
            await _service.ReactAsync(
                payload.Message.Id, _currentUserId, payload.Emoji);

            await LoadMessagesAsync();
        });
    }

    [RelayCommand]
    private async Task RemoveReactionAsync(DiscussionMessageItemViewModel? item)
    {
        if (item is null) return;

        await RunGuardedAsync(async () =>
        {
            await _service.RemoveReactionAsync(item.Id, _currentUserId);

            await LoadMessagesAsync();
        });
    }

    // ── Admin: Mute / Unmute ─────────────────────────────────────────────────

    [RelayCommand]
    private async Task MuteUserAsync(MutePayload? payload)
    {
        if (payload is null) return;

        await RunGuardedAsync(async () =>
        {
            await _service.MuteUserAsync(
                _event.EventId,
                payload.TargetUserId,
                payload.Until,
                _currentUserId);
        });
    }

    [RelayCommand]
    private async Task UnmuteUserAsync(int targetUserId)
    {
        await RunGuardedAsync(async () =>
        {
            await _service.UnmuteUserAsync(
                _event.EventId, targetUserId, _currentUserId);
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

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
            ErrorMessage = "You don't have permission for this action.";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("========== FULL EXCEPTION ==========");
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            System.Diagnostics.Debug.WriteLine("=====================================");
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnIsLoadingChanged(bool value) => NotifyCommandsChanged();
    partial void OnNewMessageChanged(string value) => NotifyCommandsChanged();
    partial void OnMediaPathChanged(string? value) => NotifyCommandsChanged();
    partial void OnIsMutedChanged(bool value) => NotifyCommandsChanged();

    private void NotifyCommandsChanged()
    {
        SendMessageCommand.NotifyCanExecuteChanged();
    }
}
