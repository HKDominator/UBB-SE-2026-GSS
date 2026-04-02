// ViewModels/StatisticsViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using Events_GSS.Data.Models;
using Events_GSS.Data.Services.Interfaces;

namespace Events_GSS.ViewModels
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private readonly IStatisticsService _statisticsService;

        private ObservableCollection<LeaderboardEntry> _leaderboard = new();
        public ObservableCollection<LeaderboardEntry> Leaderboard
        {
            get => _leaderboard;
            private set { _leaderboard = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        // The event whose leaderboard is currently displayed
        private Event? _currentEvent;
        public Event? CurrentEvent
        {
            get => _currentEvent;
            private set { _currentEvent = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand { get; }

        public StatisticsViewModel(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
            LoadCommand = new RelayCommandStats(async p => await LoadLeaderboardAsync(p));
        }

        public async Task LoadLeaderboardAsync(object? parameter)
        {
            if (parameter is not Event ev) return;
            CurrentEvent = ev;
            await LoadLeaderboardAsync(ev.EventId);
        }

        public async Task LoadLeaderboardAsync(int eventId)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

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
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RelayCommandStats : ICommand
    {
        private readonly Func<object?, Task> _executeAsync;
        private readonly Func<object?, bool>? _canExecute;
        private bool _isExecuting;

        public RelayCommandStats(Func<object?, Task> executeAsync, Func<object?, bool>? canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
            => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;
            _isExecuting = true;
            RaiseCanExecuteChanged();
            try { await _executeAsync(parameter); }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}