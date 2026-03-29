using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

using Events_GSS.Data.Models;
using Events_GSS.Data.Services;

namespace Events_GSS.ViewModels
{
    public class MemoryViewModel : INotifyPropertyChanged
    {
        private readonly IMemoryService _memoryService;
        private readonly IUserService _userService;

        private int _eventId;
        private User _currentUser = null!;

        private ObservableCollection<Memory> _memories = new();
        private ObservableCollection<string> _galleryPhotos = new();
        private bool _showOnlyMine;
        private bool _isLoading;
        private string? _errorMessage;

       

        public ObservableCollection<Memory> Memories
        {
            get => _memories;
            private set { _memories = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> GalleryPhotos
        {
            get => _galleryPhotos;
            private set { _galleryPhotos = value; OnPropertyChanged(); }
        }

        public bool ShowOnlyMine
        {
            get => _showOnlyMine;
            set { _showOnlyMine = value; OnPropertyChanged(); _ = LoadMemoriesAsync(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set { _isLoading = value; OnPropertyChanged(); }
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(_errorMessage);

        // ── Commands ─────────────────────────────────────────────────

        public ICommand SortAscendingCommand { get; }
        public ICommand SortDescendingCommand { get; }

        // ── Constructor ──────────────────────────────────────────────

        public MemoryViewModel(IMemoryService memoryService, IUserService userService)
        {
            _memoryService = memoryService;
            _userService = userService;

            SortAscendingCommand = new RelayCommand(async () => await SortAsync(ascending: true));
            SortDescendingCommand = new RelayCommand(async () => await SortAsync(ascending: false));
        }

        // ── Init ─────────────────────────────────────────────────────

        public async Task InitializeAsync(int eventId)
        {
            _eventId = eventId;
            _currentUser = _userService.GetCurrentUser();
            await LoadMemoriesAsync();
        }

        // ── Public methods (called from code-behind) ──────────────────

        public async Task OpenGalleryAsync()
        {
            try
            {
                var photos = await _memoryService.GetOnlyPhotosAsync(_eventId);
                GalleryPhotos = new ObservableCollection<string>(photos);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load gallery: {ex.Message}";
            }
        }

        public async Task AddMemoryAsync(string? photoPath, string? text)
        {
            ErrorMessage = null;
            try
            {
                await _memoryService.AddAsync(_eventId, _currentUser.UserId, photoPath, text);
                await LoadMemoriesAsync();
            }
            catch (InvalidOperationException ex) { ErrorMessage = ex.Message; }
            catch (Exception ex) { ErrorMessage = $"Could not add memory: {ex.Message}"; }
        }

        public async Task DeleteMemoryAsync(int memoryId)
        {
            ErrorMessage = null;
            try
            {
                await _memoryService.DeleteAsync(memoryId, _currentUser.UserId);
                await LoadMemoriesAsync();
            }
            catch (UnauthorizedAccessException ex) { ErrorMessage = ex.Message; }
            catch (Exception ex) { ErrorMessage = $"Could not delete memory: {ex.Message}"; }
        }

        public async Task ToggleLikeAsync(int memoryId)
        {
            ErrorMessage = null;
            try
            {
                await _memoryService.ToggleLikeAsync(memoryId, _currentUser.UserId);
                await LoadMemoriesAsync();
            }
            catch (InvalidOperationException ex) { ErrorMessage = ex.Message; }
            catch (Exception ex) { ErrorMessage = $"Could not toggle like: {ex.Message}"; }
        }

        // ── Private methods ──────────────────────────────────────────

        private async Task LoadMemoriesAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                List<Memory> list = _showOnlyMine
                    ? await _memoryService.FilterByMyMemoriesAsync(_eventId, _currentUser.UserId)
                    : await _memoryService.GetByEventAsync(_eventId, _currentUser.UserId);

                Memories = new ObservableCollection<Memory>(list);
            }
            catch (Exception ex) { ErrorMessage = $"Could not load memories: {ex.Message}"; }
            finally { IsLoading = false; }
        }

        private async Task SortAsync(bool ascending)
        {
            IsLoading = true;
            try
            {
                var sorted = await _memoryService.OrderByDateAsync(_eventId, _currentUser.UserId, ascending);
                Memories = new ObservableCollection<Memory>(sorted);
            }
            catch (Exception ex) { ErrorMessage = $"Could not sort: {ex.Message}"; }
            finally { IsLoading = false; }
        }

        // ── INotifyPropertyChanged ────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        public RelayCommand(Func<Task> execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public async void Execute(object? parameter) => await _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        public RelayCommand(Func<T, Task> execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public async void Execute(object? parameter) => await _execute((T)parameter!);
    }
}