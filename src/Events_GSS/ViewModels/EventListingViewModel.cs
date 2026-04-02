using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Events_GSS.Data.Models;
using Events_GSS.Data.Services.categoryServices;
using Events_GSS.Data.Services.eventServices;
using Events_GSS.UIServices;
using Events_GSS.Services;

using Microsoft.UI.Xaml;

namespace Events_GSS.ViewModels;


public partial class EventListingViewModel : ObservableObject
{
    private readonly IEventService _service;
    private readonly ICategoryServices _categoryServices;
    private readonly INavigationService _navigation;

    private List<Event> _allEvents = new();

    [ObservableProperty]
    private ObservableCollection<Event> _events = new();

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _locationFilter = string.Empty;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private DateTimeOffset? _dateFilter;

    [ObservableProperty]
    private DateTimeOffset? _dateRangeEnd;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private Visibility _noResultsVisibility = Visibility.Collapsed;

    public EventListingViewModel(
        IEventService service,
        ICategoryServices categoryService,
        INavigationService navigation)
    {
        _service = service;
        _categoryServices = categoryService;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task Load()
    {
        IsLoading = true;
        _allEvents = await _service.GetAllPublicActiveEventsAsync();
        var cats = await _categoryServices.GetAllCategoriesAsync();
        Categories = new ObservableCollection<Category>(cats);
        ApplyFilters();
        IsLoading = false;
    }

    [RelayCommand]
    private void Filter() => ApplyFilters();

    [RelayCommand]
    private void ClearFilters()
    {
        SearchQuery = string.Empty;
        LocationFilter = string.Empty;
        SelectedCategory = null;
        DateFilter = null;
        DateRangeEnd = null;
        ApplyFilters();
    }

    [RelayCommand]
    private void NavigateToCreate()
        => _navigation.NavigateTo("CreateEventPage");

    [RelayCommand]
    private void NavigateToAllEvents()
        => _navigation.NavigateTo("EventListingPage");

    [RelayCommand]
    private void NavigateToMyEvents()
        => _navigation.NavigateTo("MyEventsPage");

    private void ApplyFilters()
    {
        var filtered = _allEvents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
            filtered = filtered.Where(e =>
                e.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

        if (SelectedCategory != null)
            filtered = filtered.Where(e =>
                e.Category?.CategoryId == SelectedCategory.CategoryId);

        if (!string.IsNullOrWhiteSpace(LocationFilter))
            filtered = filtered.Where(e =>
                e.Location.Contains(LocationFilter, StringComparison.OrdinalIgnoreCase));

        if (DateFilter.HasValue)
            filtered = filtered.Where(e =>
                e.StartDateTime.Date >= DateFilter.Value.Date);

        if (DateRangeEnd.HasValue)
            filtered = filtered.Where(e =>
                e.StartDateTime.Date <= DateRangeEnd.Value.Date);

        var result = filtered.ToList();
        Events = new ObservableCollection<Event>(result);
        NoResultsVisibility = result.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilters();
    partial void OnLocationFilterChanged(string value) => ApplyFilters();
}