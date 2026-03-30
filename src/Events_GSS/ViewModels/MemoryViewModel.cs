using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Events_GSS.Data.Models;
using Events_GSS.Data.Services;
namespace Events_GSS.ViewModels {

    public class MemoryViewModel : INotifyPropertyChanged
    {
        private readonly IMemoryService _memoryService;
        public ICommand SortAscendingCommand { get; }
        public ICommand SortDescendingCommand { get; }

        private Event _event = null!;
        private User _currentUser = null!;

        private ObservableCollection<Memory> _memories = new();
        private ObservableCollection<string> _galleryPhotos = new();

        private bool _showOnlyMine;
        private bool _isLoading;
        private string? _errorMessage;
        private bool? _sortAscending = false;

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

        public bool ShowOnlyMine
        {
            get => _showOnlyMine;
            set
            {
                _showOnlyMine = value;
                OnPropertyChanged();
                _ = LoadMemoriesAsync();
            }
        }

        public MemoryViewModel(IMemoryService memoryService)
        {
            _memoryService = memoryService;

            SortAscendingCommand = new RelayCommand(async () => await SortAsync(ascending: true));
            SortDescendingCommand = new RelayCommand(async () => await SortAsync(ascending: false));
        }

        public async Task InitializeAsync(Event currentEvent, User currentUser)
        {
            _event = currentEvent;
            _currentUser = currentUser;
            await LoadMemoriesAsync();
        }

        public async Task OpenGalleryAsync()
        {
            try
            {
                var photos = await _memoryService.GetOnlyPhotosAsync(_event);
                GalleryPhotos = new ObservableCollection<string>(photos);
            }
            catch (Exception ex) { ErrorMessage = $"Could not load gallery: {ex.Message}"; }
        }

        public async Task AddMemoryAsync(string? photoPath, string? text)
        {
            ErrorMessage = null;
            try
            {
                await _memoryService.AddAsync(_event, _currentUser, photoPath, text);
                await LoadMemoriesAsync();
            }
            catch (InvalidOperationException ex) { ErrorMessage = ex.Message; }
            catch (UnauthorizedAccessException ex) { ErrorMessage = ex.Message; }
            catch (Exception ex) { ErrorMessage = $"Could not add memory: {ex.Message}"; }
        }

        public async Task DeleteMemoryAsync(Memory memory)
        {
            ErrorMessage = null;
            try
            {
                await _memoryService.DeleteAsync(memory, _currentUser);
                await LoadMemoriesAsync();
            }
            catch (UnauthorizedAccessException ex) { ErrorMessage = ex.Message; }
            catch (Exception ex) { ErrorMessage = $"Could not delete memory: {ex.Message}"; }
        }

        public async Task ToggleLikeAsync(Memory memory)
        {
            ErrorMessage = null;
            try
            {
                await _memoryService.ToggleLikeAsync(memory, _currentUser);
                await LoadMemoriesAsync();
            }
            catch (InvalidOperationException ex) { ErrorMessage = ex.Message; }
            catch (Exception ex) { ErrorMessage = $"Could not toggle like: {ex.Message}"; }
        }

        public void ResetOnlyMineWithoutReload()
        {
            _showOnlyMine = false;
            _sortAscending = false; // reset la default desc
            OnPropertyChanged(nameof(ShowOnlyMine));
        }

        public bool IsOwnMemory(Memory memory)
        {
            return _memoryService.IsOwnMemory(memory, _currentUser);
        }

        private async Task LoadMemoriesAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                List<Memory> list;

                if (_showOnlyMine)
                    list = await _memoryService.FilterByMyMemoriesAsync(_event, _currentUser);
                else
                    list = await _memoryService.OrderByDateAsync(_event, _currentUser, _sortAscending ?? false);

                Memories = new ObservableCollection<Memory>(list);
            }
            catch (Exception ex) { ErrorMessage = $"Could not load memories: {ex.Message}"; }
            finally { IsLoading = false; }
        }

        private async Task SortAsync(bool ascending)
        {
            _sortAscending = ascending;
            await LoadMemoriesAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
