using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

using Events_GSS.Data.Models;
using Events_GSS.Data.Services;

using Microsoft.UI.Xaml;

namespace Events_GSS.ViewModels
{
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
      

        public MemoryViewModel(IMemoryService memoryService)
        {
            _memoryService = memoryService;


           SortAscendingCommand = new RelayCommand(async () => await SortAsync(ascending: true));
           SortDescendingCommand = new RelayCommand(async () => await SortAsync(ascending: false));
        }
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
                await _memoryService.AddAsync(_event, _currentUser, photoPath, text);
                await LoadMemoriesAsync();
            }
            catch (InvalidOperationException ex) { ErrorMessage = ex.Message; }
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

      

        private async Task LoadMemoriesAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                List<Memory> list = _showOnlyMine
                    ? await _memoryService.FilterByMyMemoriesAsync(_event, _currentUser)
                    : await _memoryService.GetByEventAsync(_event, _currentUser);

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
                var sorted = await _memoryService.OrderByDateAsync(_event, _currentUser, ascending);
                Memories = new ObservableCollection<Memory>(sorted);

            }
            catch (Exception ex) { ErrorMessage = $"Could not sort: {ex.Message}"; }
            finally { IsLoading = false; }
        }

        public void ResetOnlyMineWithoutReload()
        {
            _showOnlyMine = false;
            OnPropertyChanged(nameof(ShowOnlyMine)); 
                                                    
        }

        //INotifyPropertyChanged 

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public bool IsOwnMemory(Memory memory)
        {
            return _memoryService.IsOwnMemory(memory, _currentUser);
        }
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