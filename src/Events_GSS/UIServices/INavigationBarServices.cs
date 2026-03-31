using System;
using System.Collections.Generic;
using System.Text;

namespace Events_GSS.UIServices;

public interface INavigationServices
{
    void NavigateTo(string pageKey);
    void NavigateTo(string pageKey, object parameter);
    void GoBack();
}
