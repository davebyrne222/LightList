using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using LightList.Repositories;
using LightList.Services;
using LightList.ViewModels;
using LightList.Views;
using LightList.Views.Components;

namespace LightList;

public static class MauiProgram
{
    static IServiceProvider serviceProvider;

    public static TService GetService<TService>()
        => serviceProvider.GetService<TService>();
    
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
        builder.Services.AddSingleton<AllTasksViewModel>();
        builder.Services.AddSingleton<NavBarViewModel>();
        builder.Services.AddSingleton<ITaskViewModelFactory, TaskViewModelFactory>();
        builder.Services.AddTransient<TaskViewModel>();
        builder.Services.AddTransient<TaskListViewModel>();

        // Register Views
        builder.Services.AddSingleton<AllTasksPage>();
        builder.Services.AddTransient<TaskPage>();
        
        // Register View Components
        builder.Services.AddTransient<TaskListView>();
        builder.Services.AddSingleton<NavBar>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        serviceProvider = app.Services;
        
        return app;
    }
}