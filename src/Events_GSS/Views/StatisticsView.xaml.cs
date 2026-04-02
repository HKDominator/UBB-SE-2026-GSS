using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Events_GSS.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace Events_GSS.Views;

public sealed partial class StatisticsView : Page
{
    public StatisticsViewModel ViewModel { get; }

    public StatisticsView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<StatisticsViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is int eventId)
            await ViewModel.LoadLeaderboardAsync(eventId);
    }
}