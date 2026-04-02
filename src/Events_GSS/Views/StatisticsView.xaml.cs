using Events_GSS.Data.Models;
using Events_GSS.Data.Services.Interfaces;
using Events_GSS.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Events_GSS.Views
{
    public sealed partial class StatisticsView : Page
    {
        public StatisticsViewModel ViewModel { get; private set; } = null!;

        public StatisticsView()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ViewModel is null)
            {
                var statisticsService = App.Services.GetRequiredService<IStatisticsService>();
                ViewModel = new StatisticsViewModel(statisticsService);
                DataContext = ViewModel;
            }

            // e.Parameter poate fi un Event (navigat din EventDetail)
            // sau un int (eventId direct)
            if (e.Parameter is Event ev)
                await ViewModel.LoadLeaderboardAsync(ev.EventId);
            else if (e.Parameter is int eventId)
                await ViewModel.LoadLeaderboardAsync(eventId);
        }
    }
}