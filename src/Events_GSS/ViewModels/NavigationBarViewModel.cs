using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Events_GSS.Services;


namespace Events_GSS.ViewModels;

public partial class NavigationBarViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public NavigationBarViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private void NavigateToAllEvents()
        => _navigation.NavigateTo(PageKeys.EventListing);

    [RelayCommand]
    private void NavigateToMyEvents()
        => _navigation.NavigateTo(PageKeys.MyEvents);
}
