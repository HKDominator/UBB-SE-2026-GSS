using Events_GSS.Data.Database;

using Microsoft.UI.Xaml;

using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories;
using Events_GSS.Data.Services.Interfaces;
using Events_GSS.Data.Services; 
using Events_GSS.ViewModels;

namespace Events_GSS;

public sealed partial class MainWindow : Window
{
    public QuestAdminViewModel QuestViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();

        //TODO PlaceHolder for event data, replace when navigation is implemented
        var currentEvent = new Event { Id = 1 };

        IQuestService questService = new QuestService(
            new QuestRepository(
                new SqlConnectionFactory(
                    "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ISSEvents;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30"
                )
            )
        ); 

        QuestViewModel = new QuestAdminViewModel(currentEvent, questService);
    }
}