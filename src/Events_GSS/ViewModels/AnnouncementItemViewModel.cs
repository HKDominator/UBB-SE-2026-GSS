using CommunityToolkit.Mvvm.ComponentModel;

using Events_GSS.Data.Models;

namespace Events_GSS.ViewModels;

public partial class AnnouncementItemViewModel : ObservableObject
{
    public Announcement Model { get; }

    public AnnouncementItemViewModel(Announcement model)
    {
        Model = model;
        _isRead = model.IsRead;
        _isExpanded = false;
    }

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isRead;

    // Expose model properties
    public int Id => Model.Id;
    public string Message => Model.Message;
    public DateTime Date => Model.Date;
    public bool IsPinned => Model.IsPinned;
    public bool IsEdited => Model.IsEdited;
    public User? Author => Model.Author;
    public Event? Event => Model.Event;
}