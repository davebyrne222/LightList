using System.ComponentModel;
using System.Runtime.CompilerServices;
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
    
    #endregion
    
    #region Init

    public AppShell(IAuthService authService)
    {
        Logger.Log("Initializing");

        InitializeComponent();
        
        _authService = authService;
        BindingContext = this;

        RegisterRoutes();
        _ = GetLoginStatus();
        
        // Todo: Change to messages
        // Temporary: check user logged in status when flyout opens
        //  Allows displaying of login or logout button
        PropertyChanged += OnPropertyChangedHandler;
        
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
        double fontSize = 14;
        var toast = Toast.Make(message, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }
    
    #endregion
    
    #region Event Handlers
    
    private async void OnPropertyChangedHandler(object? sender, PropertyChangedEventArgs e)
    {
        Logger.Log($"Property changed: {e.PropertyName}");
        
        if (e.PropertyName == nameof(FlyoutIsPresented))
        {
            Logger.Log($"FlyoutIsPresented changed to: {FlyoutIsPresented}");
            
            if (FlyoutIsPresented)
                await GetLoginStatus();
        }
    }
    
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        Logger.Log("Navigating to the login page");
        // CloseFlyout();
        // await Current.GoToAsync("LoginPage");
        IsLoggedIn = await _authService.SignInAsync();

        if (IsLoggedIn)
        {
            ShowToast("Signed In Successfully. Happy Tasking");
            // await CloseFlyout();
        }
        else
        {
            ShowToast("There was a problem signing in. Please try again.");
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
            ShowToast("There was a problem signing out. Please try again.");
        }
    }
    
    #endregion
}