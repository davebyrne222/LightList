using LightList.Models;

namespace LightList.Services;

public interface IAuthService
{
    Task<bool> SignInAsync();
    void SignOutAsync();
    Task<bool> IsUserLoggedIn();
}