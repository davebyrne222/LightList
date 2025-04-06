using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Data;
using LightList.Models;
using LightList.Repositories;
using LightList.Services;
using LightList.Utils;
using LightList.ViewModels;
using LightList.Views;
using LightList.Views.Components;
using Microsoft.Extensions.Logging;
using ILogger = LightList.Utils.ILogger;
using Task = System.Threading.Tasks.Task;

namespace LightList;

public static class MauiProgram
{
    private static IServiceProvider _serviceProvider = null!;
    private static LoggerContext _loggerContext = null!;
    private static ILogger _logger = null!;

    public static TService? GetService<TService>()
    {
        return _serviceProvider.GetService<TService>();
    }

    public static MauiApp CreateMauiApp()
    {
        Console.WriteLine("Creating MauiApp & Registering Services");

        var app = CreateBuilder();
        _serviceProvider = app.Services;
        _loggerContext = _serviceProvider.GetService<LoggerContext>()!;
        _logger = _serviceProvider.GetService<ILogger>()!;
        
        _loggerContext.Group = "App Creation";

        try
        {
            Task.Run(async () => await StartUpAsync()).Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during init: {ex.GetType().FullName} - {ex.Message}");
            throw;
        }

        _loggerContext.Reset();
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
        builder.Services.AddTransient<AuthTokens>();
        builder.Services.AddTransient<AppSyncUserTasks>();
        builder.Services.AddTransient<AppSyncUserTask>();
        builder.Services.AddTransient<AppSyncGenericResponse>();
        builder.Services.AddTransient<AppSyncErrorObject>();

        // Register ViewModels
        builder.Services.AddSingleton<NavBarViewModel>();
        builder.Services.AddTransient<TaskViewModel>();
        builder.Services.AddSingleton<ITaskViewModelFactory, TaskViewModelFactory>();
        builder.Services.AddSingleton<BaseTasksViewModel>();
        builder.Services.AddSingleton<AllTasksViewModel>();
        builder.Services.AddSingleton<TasksByDueDateViewModel>();
        builder.Services.AddSingleton<TasksByLabelViewModel>();

        // Register Views
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<TaskPage>();
        builder.Services.AddSingleton<AllTasksPage>();
        builder.Services.AddSingleton<TasksByDueDatePage>();
        builder.Services.AddSingleton<TasksByLabelPage>();

        // Register View Components
        builder.Services.AddScoped<TaskListView>();
        builder.Services.AddSingleton<NavBar>();

        // Utils
        builder.Services.AddSingleton<LoggerContext>();
        builder.Services.AddSingleton<ILogger, Logger>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }

    private static async Task StartUpAsync()
    {
        _logger.Debug("Starting init routine");

        try
        {
            await InitializeDatabaseAsync();
        }
        catch (Exception ex)
        {
            _logger.Error($"Init routine failed: {ex.GetType().FullName} {ex.Message}");
            throw;
        }
    }

    private static async Task InitializeDatabaseAsync()
    {
        _logger.Debug("Initializing Database");

        try
        {
            var dbService = GetService<TasksDatabase>();
            if (dbService is not null)
                await dbService.InitialiseAsync();
        }
        catch (Exception ex)
        {
            _logger.Critical($"Error initialising database: {ex.GetType().FullName} - {ex.Message}");
            throw;
        }

        _logger.Debug("Database initialized");
    }
}