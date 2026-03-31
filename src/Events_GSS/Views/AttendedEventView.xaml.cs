using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Events_GSS.ViewModels;

namespace Events_GSS.Views
{
    public sealed partial class AttendedEventView : Page
    {
        public AttendedEventViewModel ViewModel { get; }

        public AttendedEventView(AttendedEventViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadAsync();
        }
    }
}