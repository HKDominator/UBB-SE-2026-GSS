using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Events_GSS.UIServices;


namespace Events_GSS.ViewModels;

public partial class NavigationBarViewModel : ObservableObject
{
    private readonly INavigationServices _navigation;

    public NavigationBarViewModel(INavigationServices navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private void NavigateToAllEvents()
        => _navigation.NavigateTo("EventListingPage");

    [RelayCommand]
    private void NavigateToMyEvents()
        => _navigation.NavigateTo("MyEventsPage");
}
