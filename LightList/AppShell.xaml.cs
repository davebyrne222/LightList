using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Utils;
using LightList.Views;

namespace LightList;

public partial class AppShell : Shell
{
    #region Fields

    private readonly LoggerContext _loggerContext;
    private readonly ILogger _logger;
    private readonly IAuthService _authService;
    private bool _isLoggedIn;

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set
        {
            if (_isLoggedIn != value)
            {
                _isLoggedIn = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLoggedOut));
            }
        }
    }

    public bool IsLoggedOut => !IsLoggedIn;

    private readonly ITasksService _tasksService;
    private readonly IMessenger _messenger;

    #endregion

    #region Init

    public AppShell(LoggerContext context, ILogger logger, IAuthService authService, ITasksService tasksService, IMessenger messenger)
    {
        _loggerContext = context;
        _logger = logger;
        
        _loggerContext.Group = "Shell init";
        _logger.Debug("Initializing");

        InitializeComponent();

        _authService = authService;
        _tasksService = tasksService;
        _messenger = messenger;
        BindingContext = this;

        RegisterRoutes();
        
        _logger.Debug("Initialized");
        _loggerContext.Reset();
    }
    
    private void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(TaskPage), typeof(TaskPage));
        Routing.RegisterRoute(nameof(AllTasksPage), typeof(AllTasksPage));
        Routing.RegisterRoute(nameof(TasksByDueDatePage), typeof(TasksByDueDatePage));
        Routing.RegisterRoute(nameof(TasksByLabelPage), typeof(TasksByLabelPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
    }

    #endregion

    #region Event Handlers
    
    protected override async void OnAppearing()
    {
        _loggerContext.Group = "Shell Appearing";
        _logger.Debug("Appearing");
        
        base.OnAppearing();
        
        _loggerContext.Group = "- Login";
        await GetLoginStatus();
        
        _loggerContext.Group = "- Sync";
        if (IsLoggedIn)
            await SyncTasks();
        else
            _logger.Debug("Not logged in. Skipping sync");
        
        _loggerContext.Reset();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        _logger.Debug("Navigating to the login page");

        IsLoggedIn = await _authService.SignInAsync();

        if (IsLoggedIn)
        {
            ShowToast("Signed In Successfully. Synchronizing.");
            await SyncTasks();
            // await CloseFlyout(); 
        }
        else
        {
            ShowToast("There was a problem signing in. Please try again.", ToastDuration.Long);
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        IsLoggedIn = !await _authService.SignOutAsync();

        if (!IsLoggedIn)
            ShowToast("Signed Out Successfully");
        // await CloseFlyout();
        else
            ShowToast("There was a problem signing out. Please try again.", ToastDuration.Long);
    }

    private async void OnSyncClicked(object sender, EventArgs e)
    {
        ShowToast("Syncing...");
        await SyncTasks();
        ShowToast("Syncing complete");
    }
    
    #endregion
    
    #region Utils

    private async Task CloseFlyout()
    {
        Current.FlyoutBehavior = FlyoutBehavior.Disabled;
        await Task.Delay(1000); // Small delay to allow animation
        Current.FlyoutBehavior = FlyoutBehavior.Flyout;
    }

    private async Task GetLoginStatus()
    {
        IsLoggedIn = await _authService.IsUserLoggedIn();
        _logger.Debug($"User is logged in: {IsLoggedIn}");
    }

    private async void ShowToast(string message, ToastDuration duration = ToastDuration.Short)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var toast = Toast.Make(message, duration);
        await toast.Show(cancellationTokenSource.Token);
    }
    
    private async Task SyncTasks()
    {
        _logger.Debug("Syncing tasks");
        try
        {
            await _tasksService.SyncNowAsync();
            _logger.Debug("Finished syncing");

        }
        catch (Exception ex)
        {
            _logger.Error($"Syncing tasks failed: {ex}");
            ShowToast("There was a problem synchronising tasks. Please try again.", ToastDuration.Long);
        }
    }

    #endregion
}