using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Microsoft.Extensions.DependencyInjection;

using Events_GSS.Services;

namespace Events_GSS.Views;

public sealed partial class ShellPage : Page
{
    private readonly INavigationService _nav;

    public bool CanGoBack => _nav.CanGoBack;

    public ShellPage()
    {
        InitializeComponent();

        _nav = App.Services.GetRequiredService<INavigationService>();

        // Give the NavigationService access to the Frame
        if (_nav is NavigationService concreteNav)
        {
            concreteNav.SetFrame(ContentFrame);
        }

        // Navigate to the default page
        _nav.NavigateTo(PageKeys.EventListing);

        // Select the first menu item visually
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void OnNavItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer?.Tag is string pageKey)
        {
            _nav.NavigateTo(pageKey);
        }
    }

    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        _nav.GoBack();
    }
}
