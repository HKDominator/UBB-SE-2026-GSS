using Events_GSS.Data.Models;
using Events_GSS.ViewModels;
using Events_GSS.Services.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Events_GSS.Views;

public sealed partial class AttendedEventView : Page
{
    public AttendedEventViewModel ViewModel { get; private set; } = null!;

    // Parameterless constructor required for Frame.Navigate
    public AttendedEventView()
    {
        this.InitializeComponent();
    }

    // Keep the old constructor so existing code doesn't break if used directly
    public AttendedEventView(AttendedEventViewModel viewModel) : this()
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // If ViewModel wasn't set via constructor (navigation case), create it
        if (ViewModel is null)
        {
            var attendedEventService = App.Services.GetRequiredService<IAttendedEventService>();
            var userService = App.Services.GetRequiredService<IUserService>();
            ViewModel = new AttendedEventViewModel(attendedEventService, userService);
            DataContext = ViewModel;
        }

        await ViewModel.LoadAsync();
    }

    private async void ArchiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is AttendedEvent ae)
            await ViewModel.SetArchivedAsync(ae);
    }

    private async void FavouriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is AttendedEvent ae)
            await ViewModel.SetFavouriteAsync(ae);
    }

    private async void LeaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is AttendedEvent ae)
            await ViewModel.LeaveAsync(ae);
    }
}
