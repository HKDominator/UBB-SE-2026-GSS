using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.UI.Xaml.Controls;

namespace Events_GSS.UIServices;

public class NavigationServices : INavigationServices
{
    private Frame? _frame;

    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }

    public void NavigateTo(string pageKey)
    {
        var pageType = Type.GetType($"Events_GSS.Views.{pageKey}");
        if (pageType != null)
            _frame.Navigate(pageType);
    }

    public void NavigateTo(string pageKey, object parameter)
    {
        var pageType = Type.GetType($"Events_GSS.Views.{pageKey}");
        if (pageType != null)
            _frame.Navigate(pageType, parameter);
    }

    public void GoBack()
    {
        if (_frame.CanGoBack)
            _frame.GoBack();
    }
}