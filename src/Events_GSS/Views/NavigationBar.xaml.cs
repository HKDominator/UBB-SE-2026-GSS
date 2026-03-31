using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

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

using Events_GSS.ViewModels;

namespace Events_GSS.Views;

public sealed partial class NavigationBar : UserControl
{
    public NavigationBarViewModel ViewModel { get; }

    public NavigationBar(NavigationBarViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
    }
}