using System;

using CommunityToolkit.Mvvm.ComponentModel;

using Events_GSS.Data.Models;

namespace Events_GSS.ViewModels;

public partial class DiscussionMessageItemViewModel : ObservableObject
{
    public DiscussionMessage Model { get; }

    public DiscussionMessageItemViewModel(DiscussionMessage model)
    {
        Model = model;
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

    // UI-only state
    [ObservableProperty]
    private bool _isOriginalDeleted;
}
