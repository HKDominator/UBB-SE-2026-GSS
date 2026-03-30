using System;
using System.Linq;

using Events_GSS.Data.Database;
using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories;
using Events_GSS.Data.Services;
using Events_GSS.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;

namespace Events_GSS.Views
{
    public sealed partial class MemoryView : UserControl
    {
        public MemoryViewModel ViewModel { get; }

        public MemoryView()
        {
            this.InitializeComponent();

            var factory = new SqlConnectionFactory("Server=localhost\\SQLEXPRESS;Database=EventsApp;Trusted_Connection=True;TrustServerCertificate=True;");
            var memoryRepo = new MemoryRepository(factory);
            var memoryService = new MemoryService(memoryRepo);
           // var userService = new UserService();

            ViewModel = new MemoryViewModel(memoryService);

            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.ErrorMessage))
                    ErrorText.Visibility = ViewModel.HasError
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                if (e.PropertyName == nameof(ViewModel.Memories))
                    EmptyText.Visibility = ViewModel.Memories.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
            };
        }
        public async Task LoadAsync(Event ev, User user)
        {
            await ViewModel.InitializeAsync(ev, user);
        }
        /*
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int eventId)
                await ViewModel.InitializeAsync(eventId);
        }*/

        //Filter / Sort
        
        private void MyMemoriesToggle_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowOnlyMine = MyMemoriesToggle.IsChecked == true;
        }
        private void SortAscending_Click(object sender, RoutedEventArgs e)
        {
            MyMemoriesToggle.IsChecked = false;
            ViewModel.ResetOnlyMineWithoutReload(); 
            ViewModel.SortAscendingCommand.Execute(null); 
        }

        private void SortDescending_Click(object sender, RoutedEventArgs e)
        {
            MyMemoriesToggle.IsChecked = false;
            ViewModel.ResetOnlyMineWithoutReload();
            ViewModel.SortDescendingCommand.Execute(null);
        }
        /*
        private async void GalleryButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.OpenGalleryAsync();
            MemoryListScrollViewer.Visibility = Visibility.Collapsed;
            GalleryGrid.Visibility = Visibility.Visible;
        }*/
        private async void GalleryButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.OpenGalleryAsync();
            MemoryListScrollViewer.Visibility = Visibility.Collapsed;
            FiltersPanel.Visibility = Visibility.Collapsed;
            GalleryGrid.Visibility = Visibility.Visible;
        }

        private void CloseGallery_Click(object sender, RoutedEventArgs e)
        {
            GalleryGrid.Visibility = Visibility.Collapsed;
            FiltersPanel.Visibility = Visibility.Visible;
            MemoryListScrollViewer.Visibility = Visibility.Visible;
        }/*

        private void CloseGallery_Click(object sender, RoutedEventArgs e)
        {
            GalleryGrid.Visibility = Visibility.Collapsed;
            MemoryListScrollViewer.Visibility = Visibility.Visible;
        }*/

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton btn || btn.DataContext is not Memory memory)
                return;

            if (ViewModel.IsOwnMemory(memory))
            {
                btn.IsChecked = memory.IsLikedByCurrentUser;
                var dialog = new ContentDialog
                {
                    Title = "Can't like this",
                    Content = "You can't like your own memory.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            await ViewModel.ToggleLikeAsync(memory);

            if (ViewModel.HasError)
                btn.IsChecked = memory.IsLikedByCurrentUser; 
        }

        // ── Like ──────────────────────────────────────────────────────
        /* private async void LikeButton_Click(object sender, RoutedEventArgs e)
         {
             if (sender is ToggleButton btn &&
                 btn.DataContext is Memory memory)
             {
                 bool previousState = btn.IsChecked ?? false;

                 await ViewModel.ToggleLikeAsync(memory);

                 if (ViewModel.HasError)
                 {
                     //revert UI state
                     btn.IsChecked = !btn.IsChecked;
                 }
             }
         }*/
        /*
        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton btn && btn.Tag is Memory memoryId)
            {
                await ViewModel.ToggleLikeAsync(memoryId);

                if (ViewModel.HasError)
                {
                    // revert toggle visually if error (e.g. tried to like own memory)
                    btn.IsChecked = !btn.IsChecked;
                }
            }
        }*/

        // ── Delete ────────────────────────────────────────────────────

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not Memory memoryId)
                return;

            var dialog = new ContentDialog
            {
                Title = "Delete Memory",
                Content = "Are you sure you want to delete this memory?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                await ViewModel.DeleteMemoryAsync(memoryId);
        }

        // ── Add ───────────────────────────────────────────────────────

        private async void AddMemoryButton_Click(object sender, RoutedEventArgs e)
        {
            var photoPathBox = new TextBox
            {
                PlaceholderText = "Photo path (optional, e.g. C:\\Photos\\photo.jpg)",
                Margin = new Thickness(0, 0, 0, 8)
            };
            var textBox = new TextBox
            {
                PlaceholderText = "Write something about this memory... (optional)",
                AcceptsReturn = true,
                Height = 120,
                TextWrapping = TextWrapping.Wrap
            };

            var panel = new StackPanel { Spacing = 4 };
            panel.Children.Add(new TextBlock { Text = "Photo path", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(photoPathBox);
            panel.Children.Add(new TextBlock { Text = "Text", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(textBox);
            panel.Children.Add(new TextBlock
            {
                Text = "At least one of photo or text is required.",
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                Margin = new Thickness(0, 4, 0, 0)
            });

            var dialog = new ContentDialog
            {
                Title = "Add Memory",
                Content = panel,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                await ViewModel.AddMemoryAsync(photoPathBox.Text, textBox.Text);
        }
      
    }
}