using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using LightList.Repositories;
using LightList.Services;
using LightList.ViewModels;
using LightList.Views;

namespace LightList;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("ionicons.ttf", "Ionicons");
            });
        
        // Register Repositories
        builder.Services.AddSingleton<ILocalRepository, LocalRepository>();

        // Register Services
        builder.Services.AddSingleton<ITasksService, TasksService>();
        
        // Messenger
        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        
        // Register Models
        builder.Services.AddTransient<Models.Task>();
        
        // Register ViewModels
        builder.Services.AddSingleton<TasksViewModel>();
        builder.Services.AddSingleton<ITaskViewModelFactory, TaskViewModelFactory>();
        builder.Services.AddTransient<TaskViewModel>();

        // Register Views
        builder.Services.AddSingleton<AllTasksPage>();
        builder.Services.AddTransient<TaskPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}