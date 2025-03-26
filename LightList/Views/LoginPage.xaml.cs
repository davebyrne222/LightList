using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightList.Services;
using LightList.Utils;

namespace LightList.Views;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _authService;
    
    public LoginPage(IAuthService authService)
    {
        Logger.Log("Initializing");
        InitializeComponent();
        _authService = authService;
        Logger.Log("Initialized");
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await ManageLogin();
    }

    private async Task ManageLogin()
    {
     
        bool loggedIn = await LoginAsync();

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
        Logger.Log("Logging in");
        
        try
        {
            bool loggedIn = await _authService.SignInAsync();
            Logger.Log($"Successfully Logged In: {loggedIn}");
            return loggedIn;
        }
        catch (Exception ex)
        {
            Logger.Log($"AuthError: {ex.Message}");
            
            // Ensure UI is presented - web authenticator view may still be visible
            MainThread.BeginInvokeOnMainThread(async void () =>
            {
                await DisplayAlert("AuthError", ex.Message, "OK");
            });
            
            return false;
        }
    }

    public async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        Logger.Log("Login button clicked");
        await ManageLogin();
    }
    
    public async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"..", true);
    }
    
}