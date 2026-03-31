using System;

using CommunityToolkit.Mvvm.ComponentModel;

using Events_GSS.Data.Models;

namespace Events_GSS.ViewModels;

public partial class DiscussionMessageItemViewModel : ObservableObject
{
    public DiscussionMessage Model { get; }
    private readonly int _currentUserId;

    public DiscussionMessageItemViewModel(DiscussionMessage model, int currentUserId)
    {
        Model = model;
        _currentUserId = currentUserId;
    }

    // Expose model properties
    public int Id => Model.Id;
    public string? Message => Model.Message;
    public string? MediaPath => Model.MediaPath;
    public DateTime Date => Model.Date;
    public bool IsEdited => Model.IsEdited;
    public bool CanDelete => Model.CanDelete;
    public User? Author => Model.Author;
    public DiscussionMessage? ReplyTo => Model.ReplyTo;

    public List<ReactionGroup> ReactionGroups =>
        Model.Reactions
            .GroupBy(r => r.Emoji)
            .Select(g => new ReactionGroup
            {
                Emoji = g.Key,
                Count = g.Count(),
                CurrentUserReacted = g.Any(r => r.Author.UserId == _currentUserId)
            })
            .ToList();
    public bool HasReactions => Model.Reactions.Count > 0;
    public string? CurrentUserEmoji =>
    Model.Reactions
        .FirstOrDefault(r => r.Author.UserId == _currentUserId)?
        .Emoji;

    // UI-only state
    [ObservableProperty]
    private bool _isOriginalDeleted;
}
