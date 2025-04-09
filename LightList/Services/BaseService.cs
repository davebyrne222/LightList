using LightList.Utils;

namespace LightList.Services;

public abstract class BaseService
{
    protected readonly ILogger _logger;

    protected BaseService(ILogger logger)
    {
        _logger = logger;
    }

    protected async Task ExecuteWithLogging(Func<Task> action, string context)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            _logger.Error($"{context}: {ex.GetType()} - {ex.Message}");
            throw;
        }
    }

    protected async Task<T> ExecuteWithLogging<T>(Func<Task<T>> action, string context)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            _logger.Error($"{context}: {ex.GetType()} - {ex.Message}");
            throw;
        }
    }
}