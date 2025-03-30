namespace LightList.Services;

public interface ISyncService
{
    Task PullChangesAsync();
}