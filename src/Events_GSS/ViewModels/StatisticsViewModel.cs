using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Events_GSS.Data.Models;
using Events_GSS.Data.Services.Interfaces;

namespace Events_GSS.ViewModels;

public class StatisticsViewModel : INotifyPropertyChanged
{
    private readonly IStatisticsService _statisticsService;

    private ObservableCollection<LeaderboardEntry> _leaderboard = new();
    private bool _isLoading;
    private string _errorMessage = "";

    public ObservableCollection<LeaderboardEntry> Leaderboard
    {
        get => _leaderboard;
        private set { _leaderboard = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set { _errorMessage = value; OnPropertyChanged(); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public StatisticsViewModel(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    public async Task LoadLeaderboardAsync(int eventId)
    {
        IsLoading = true;
        ErrorMessage = "";

        try
        {
            var entries = await _statisticsService.GetLeaderboardAsync(eventId);
            Leaderboard = new ObservableCollection<LeaderboardEntry>(entries);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load leaderboard: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
