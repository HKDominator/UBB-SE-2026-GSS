using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Events_GSS.Data.Database;
using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories;
using Events_GSS.Data.Repositories.Interfaces;
using Events_GSS.Data.Services;
using Events_GSS.Data.Services.Interfaces;
using Events_GSS.Services;
using Events_GSS.Services.Interfaces;
using Events_GSS.ViewModels;
using Events_GSS.Views;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Events_GSS
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        public Window? MainWindow => _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// 
        public new static App Current => (App)Application.Current;
        public static IServiceProvider Services { get; private set; }
        public App()
        {
            InitializeComponent();
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
                .Build();
            var services = new ServiceCollection();
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddSingleton(new SqlConnectionFactory(connectionString));

            services.AddTransient<IQuestRepository,QuestRepository>();
            services.AddTransient<IQuestService,QuestService>();

            services.AddTransient<IMemoryRepository,MemoryRepository>();
            services.AddTransient<IMemoryService,MemoryService>();

            services.AddTransient<IQuestMemoryRepository, QuestMemoryRepository>();
            services.AddTransient<IQuestApprovalService, QuestApprovalService>();

            services.AddTransient<IAttendedEventRepository, AttendedEventRepository>();
            services.AddTransient<IAttendedEventService, AttendedEventService>();

            services.AddTransient<IUserService, MockUserService>();

            Services = services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            // 1. Get your services from the DI container you already set up
            var questService = Services.GetRequiredService<IQuestService>();
            var questApprovalService = Services.GetRequiredService<IQuestApprovalService>();
            // 2. Create the "Inner" ViewModel (The one for QuestAdminControl)
            // You'll need a dummy 'Event' object here since your VM requires one
            var dummyEvent = new Event { Name = "Test Event", EventId = 1 };
            var adminVM = new QuestAdminViewModel(dummyEvent, questService);

            // 3. Create the "Bridge" Page manually
            var rootPage = new QuestApprovalPage();

            // 4. Manually trigger the ViewModel setup (since OnNavigatedTo won't fire)
            rootPage.ViewModel = new QuestApprovalViewModel(adminVM,questApprovalService);

            // 5. Set the window content
            _window.Content = rootPage;
            _window.Activate();
        }
    }
}
