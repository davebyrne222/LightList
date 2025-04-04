using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Maui;
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
    private static IServiceProvider _serviceProvider = null!;

    public static TService? GetService<TService>()
        => _serviceProvider.GetService<TService>();
    
    public static MauiApp CreateMauiApp()
    {
        Logger.Log("Creating MauiApp & Registering Services");

        MauiApp app = CreateBuilder();
        
        _serviceProvider = app.Services;

        try
        {
            // _ = StartUpAsync(); // <-- async startup. App will be ready before database
            Task.Run(async () => await StartUpAsync()).Wait();
        }
        catch (Exception ex)
        {
            Logger.Log($"Error starting up: {ex.GetType().FullName} - {ex.Message}");
            throw;
        }
        
        return app;
    }

    private static MauiApp CreateBuilder()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                // fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("ionicons.ttf", "Ionicons");
            });
        
        // Register Database
        builder.Services.AddSingleton<TasksDatabase>();
        
        // Register Repositories
        builder.Services.AddSingleton<ILocalRepository, LocalRepository>();
        builder.Services.AddSingleton<ISecureStorageRepository, SecureStorageRepository>();
        builder.Services.AddSingleton<IRemoteRepository, RemoteRepository>();

        // Register Services
        builder.Services.AddSingleton<ITasksService, TasksService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<ISyncService, SyncService>();
        
        // Register Messenger
        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        
        // Register Models
        builder.Services.AddTransient<Models.Task>();
        builder.Services.AddTransient<Models.AuthTokens>();
        builder.Services.AddTransient<Models.AppSyncUserTasks>();
        builder.Services.AddTransient<Models.AppSyncUserTask>();
        builder.Services.AddTransient<Models.AppSyncGenericResponse>();
        builder.Services.AddTransient<Models.AppSyncErrorObject>();
        
        // Register ViewModels
        builder.Services.AddSingleton<NavBarViewModel>();
        builder.Services.AddTransient<TaskViewModel>();
        builder.Services.AddSingleton<ITaskViewModelFactory, TaskViewModelFactory>();
        builder.Services.AddSingleton<BaseTasksViewModel>();
        builder.Services.AddSingleton<AllTasksViewModel>();
        builder.Services.AddSingleton<TasksByDueDateViewModel>();
        builder.Services.AddSingleton<TasksByLabelViewModel>();
        
        // Register Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<TaskPage>();
        builder.Services.AddSingleton<AllTasksPage>();
        builder.Services.AddSingleton<TasksByDueDatePage>();
        builder.Services.AddSingleton<TasksByLabelPage>();
        
        // Register View Components
        builder.Services.AddScoped<TaskListView>();
        builder.Services.AddSingleton<NavBar>();
        
        // Misc
        builder.Services.AddSingleton<AppShell>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
    
    private static async Task StartUpAsync()
    {
        Logger.Log("Starting StartUp routine");

        try
        {
            await InitializeDatabaseAsync();
            
        }
        catch (Exception ex)
        {
            Logger.Log($"StartUp routine failed: {ex.GetType().FullName} {ex.Message}");
            throw;
        }
    }
    
    private static async Task InitializeDatabaseAsync()
    {
        Logger.Log("Initializing Database");
        
        try
        {
            TasksDatabase? dbService = GetService<TasksDatabase>();
            if (dbService is not null)
                await dbService.InitialiseAsync();
        }
        catch (Exception ex)
        {
            Logger.Log($"Error initialising database: {ex.GetType().FullName} - {ex.Message}");
            throw;
        }
        
        Logger.Log("Database initialized");
    }
}