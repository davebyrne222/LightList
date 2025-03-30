using System.ComponentModel;
using LightList.Services;
using LightList.Utils;
using LightList.Views;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace LightList;

public partial class AppShell : Shell
{
    #region Fields
    
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
    
    private readonly ISyncService _syncService;
    
    #endregion
    
    #region Init

    public AppShell(IAuthService authService, ISyncService syncService)
    {
        Logger.Log("Initializing");

        InitializeComponent();
        
        _authService = authService;
        _syncService = syncService;
        BindingContext = this;

        RegisterRoutes();
        _ = GetLoginStatus();
        
        Logger.Log("Initialized");
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
        Logger.Log($"User is logged in: {IsLoggedIn}");
    }

    private async void ShowToast(string message, ToastDuration duration = ToastDuration.Short)
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        var toast = Toast.Make(message, duration, 14);
        await toast.Show(cancellationTokenSource.Token);
    }
    
    #endregion
    
    #region Event Handlers
    
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        Logger.Log("Navigating to the login page");

        IsLoggedIn = await _authService.SignInAsync();

        if (IsLoggedIn)
        {
            ShowToast("Signed In Successfully. Synchronizing.");
            SyncTasks();
            // await CloseFlyout();
        }
        else
        {
            ShowToast("There was a problem signing in. Please try again.", ToastDuration.Long);
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        IsLoggedIn = ! await _authService.SignOutAsync();
        
        if (!IsLoggedIn)
        {
            ShowToast("Signed Out Successfully");
            // await CloseFlyout();
        }
        else
        {
            ShowToast("There was a problem signing out. Please try again.", ToastDuration.Long);
        }
    }

    private async void SyncTasks()
    {
        Logger.Log("Syncing tasks");
        await _syncService.SyncRemoteDataAsync();
        Logger.Log("Finished syncing");
    }
    
    #endregion
}