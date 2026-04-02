using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Events_GSS.ViewModels;
using Events_GSS.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Events_GSS.Views;

public sealed partial class EventListingPage : Page
{
    public EventListingViewModel ViewModel { get; private set; } = null!;

    public EventListingPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ViewModel = e.Parameter is EventListingViewModel vm
            ? vm
            : App.Services.GetRequiredService<EventListingViewModel>();

        ViewModel.LoadCommand.Execute(null);
    }

    private void OnEventCardTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Events_GSS.Data.Models.Event ev)
        {
            var nav = App.Services.GetRequiredService<INavigationService>();
            nav.NavigateTo(PageKeys.EventDetail, ev);
        }
    }
}