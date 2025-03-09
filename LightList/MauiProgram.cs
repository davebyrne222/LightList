using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using LightList.Repositories;
using LightList.Services;
using LightList.ViewModels;
using LightList.Views;
using LightList.Views.Components;
using LightList.Data;
using LightList.Utils;

namespace LightList;

public static class MauiProgram
{
    static IServiceProvider serviceProvider;

    public static TService GetService<TService>()
        => serviceProvider.GetService<TService>();
    
    public static MauiApp CreateMauiApp()
    {
        Logger.Log("Creating MauiApp & Registering Services");
        
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("ionicons.ttf", "Ionicons");
            });
        
        // Register Database
        builder.Services.AddSingleton<TasksDatabase>();
        
        // Register Repositories
        builder.Services.AddSingleton<ILocalRepository, LocalRepository>();

        // Register Services
        builder.Services.AddSingleton<ITasksService, TasksService>();
        
        // Messenger
        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        
        // Register Models
        builder.Services.AddTransient<Models.Task>();
        
        // Register ViewModels
        builder.Services.AddSingleton<NavBarViewModel>();
        builder.Services.AddTransient<TaskViewModel>();
        builder.Services.AddSingleton<ITaskViewModelFactory, TaskViewModelFactory>();
        builder.Services.AddSingleton<BaseTasksViewModel>();
        builder.Services.AddSingleton<AllTasksViewModel>();
        builder.Services.AddSingleton<TasksByDueDateViewModel>();
        builder.Services.AddSingleton<TasksByLabelViewModel>();
        
        // Register Views
        builder.Services.AddTransient<TaskPage>();
        builder.Services.AddSingleton<AllTasksPage>();
        builder.Services.AddSingleton<TasksByDueDatePage>();
        builder.Services.AddSingleton<TasksByLabelPage>();
        
        // Register View Components
        builder.Services.AddScoped<TaskListView>();
        builder.Services.AddSingleton<NavBar>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        serviceProvider = app.Services;
        
        // Initialize database before returning the app
        var dbService = app.Services.GetRequiredService<TasksDatabase>();
        Task.Run(async () => await dbService.InitialiseAsync()).Wait();
        
        return app;
    }
}