using LightList.Services;
using LightList.Utils;

namespace LightList.Views;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _authService;
    private readonly ILogger _logger;

    public LoginPage(ILogger logger, IAuthService authService)
    {
        _logger = logger;
        _logger.Debug("Initializing");
        InitializeComponent();
        _authService = authService;
        _logger.Debug("Initialized");
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await ManageLogin();
    }

    private async Task ManageLogin()
    {
        var loggedIn = await LoginAsync();

        if (loggedIn)
        {
            LoginStatusLbl.Text = "Successfully logged in! Happy tasking";
        }
        else
        {
            LoginStatusLbl.Text = "Whoops! Something went wrong... Please trying signing in again or try again later";
            SignInBtn.IsVisible = true;
        }
    }

    private async Task<bool> LoginAsync()
    {
        _logger.Debug("Logging in");

        try
        {
            var loggedIn = await _authService.SignInAsync();
            _logger.Debug($"Successfully Logged In: {loggedIn}");
            return loggedIn;
        }
        catch (Exception ex)
        {
            _logger.Error($"AuthError: {ex.Message}");

            // Ensure UI is presented - web authenticator view may still be visible
            MainThread.BeginInvokeOnMainThread(async void () => { await DisplayAlert("AuthError", ex.Message, "OK"); });

            return false;
        }
    }

    public async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        _logger.Debug("Login button clicked");
        await ManageLogin();
    }

    public async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..", true);
    }
}