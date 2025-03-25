using LightList.Models;

namespace LightList.Services;

public interface IAuthService
{
    Task<bool> SignInAsync();
    Task<bool> SignOutAsync();
    Task<bool> IsUserLoggedIn();
}