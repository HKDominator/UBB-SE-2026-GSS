using System;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Windows.Storage.Pickers;

using Events_GSS.ViewModels;

namespace Events_GSS.Views;

public sealed partial class DiscussionControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(DiscussionViewModel),
            typeof(DiscussionControl),
            new PropertyMetadata(null, OnViewModelChanged));

    public DiscussionViewModel? ViewModel
    {
        get => (DiscussionViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public DiscussionControl()
    {
        InitializeComponent();
    }

    // ── Auto-scroll + empty state when the collection changes ────────────────

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DiscussionControl control && e.NewValue is DiscussionViewModel vm)
        {
            vm.Messages.CollectionChanged += (_, _) =>
            {
                control.UpdateEmptyState();
                control.ScrollToBottom();
            };
        }
    }

    // ── Event handlers (bridge XAML events → ViewModel commands) ──────────────

    private void OnReplyClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe
            && fe.Tag is DiscussionMessageItemViewModel item
            && ViewModel is not null)
        {
            ViewModel.SetReplyTargetCommand.Execute(item);
        }
    }

    private void OnEmojiClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string emoji || ViewModel is null)
            return;

        // Walk up the visual tree to find the DataTemplate's DataContext
        var item = FindAncestorDataContext<DiscussionMessageItemViewModel>(btn);
        if (item is not null)
        {
            ViewModel.AddReactionCommand.Execute(
                new DiscussionReactionPayload(item, emoji));
        }
    }

    private async void OnDeleteClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe
            || fe.Tag is not DiscussionMessageItemViewModel item
            || ViewModel is null)
            return;

        // Confirmation dialog (REQ-DIS-03)
        var dialog = new ContentDialog
        {
            Title = "Delete message",
            Content = "Are you sure you want to delete this message? This cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.DeleteMessageCommand.Execute(item);
        }
    }

    private async void OnAttachMediaClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null) return;

        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".gif");
        picker.FileTypeFilter.Add(".mp4");
        picker.FileTypeFilter.Add(".mov");

        // WinUI 3 desktop requires initialising the picker with
        // the window handle. You need to expose the Window from App:
        //   public Window? MainWindow => _window;
        // If you use a different approach to get the HWND, replace
        // the two lines below accordingly.
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(
            ((App)Application.Current).MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var mediaFolder = await localFolder.CreateFolderAsync(
                "DiscussionMedia",
                Windows.Storage.CreationCollisionOption.OpenIfExists);

            var copy = await file.CopyAsync(
                mediaFolder,
                file.Name,
                Windows.Storage.NameCollisionOption.GenerateUniqueName);

            ViewModel.MediaPath = copy.Path;
        }
    }

    private void OnClearMediaClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not null)
        {
            ViewModel.MediaPath = null;
        }
    }

    private void OnMessageInputKeyDown(object sender, KeyRoutedEventArgs e)
    {
        // Enter sends the message
        if (e.Key == Windows.System.VirtualKey.Enter
            && ViewModel is not null
            && ViewModel.SendMessageCommand.CanExecute(null))
        {
            ViewModel.SendMessageCommand.Execute(null);
            e.Handled = true;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void ScrollToBottom()
    {
        if (ViewModel is null || ViewModel.Messages.Count == 0) return;

        DispatcherQueue.TryEnqueue(() =>
        {
            MessagesListView.ScrollIntoView(
                ViewModel.Messages[^1],
                ScrollIntoViewAlignment.Leading);
        });
    }

    private void UpdateEmptyState()
    {
        EmptyStateText.Visibility =
            ViewModel?.Messages.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    /// <summary>
    /// Walks up the visual tree from <paramref name="start"/> until it finds
    /// a FrameworkElement whose DataContext is of type <typeparamref name="T"/>.
    /// </summary>
    private static T? FindAncestorDataContext<T>(DependencyObject start) where T : class
    {
        var current = start;
        while (current is not null)
        {
            if (current is FrameworkElement fe && fe.DataContext is T found)
                return found;

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
