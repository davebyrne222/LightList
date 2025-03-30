using System.Text;
using System.Text.Json;
using LightList.Models;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public class RemoteRepository : IRemoteRepository
{
    private static readonly HttpClient Client = new();

    public async Task<List<Models.Task?>> GetUserTasks(AuthTokens accessToken)
    {
        string query = $$"""query { getUserTasks(UserId: "{{accessToken.UserId}}") { ItemId, Data, UpdatedAt } }""";
        string result = await ExecuteQuery(accessToken.AccessToken, query);
        return DeserializeUserTasks(result);
    }

    private async Task<string> ExecuteQuery(string accessToken, string query)
    {
        Logger.Log($"Executing query");
        
        var request = new HttpRequestMessage(HttpMethod.Post, Constants.AppSyncEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("Authorization", accessToken);
        
        var response = await Client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            string msg = $"Query failed: {response.StatusCode} - {response.ReasonPhrase}";
            Logger.Log(msg);
            throw new HttpRequestException(msg);
        }
        
        Logger.Log($"Query successful");
        
        return await response.Content.ReadAsStringAsync();
    }
    
    #region Utils
    private static List<Models.Task?> DeserializeUserTasks(string response)
    {
        Logger.Log($"Deserializing response");
        try
        {
            var result = JsonSerializer.Deserialize<AppSyncGetTasksResponse>(response);
            
            if (result is null)
                throw new NullReferenceException("Deserializing failed: Response is null");
            
            Logger.Log($"Successfully deserialize response. Converting to model");

            return result.Data.UserTasks.Select(
                ConvertToTaskModel
                ).ToList();

        }
        catch (Exception ex)
        {
            Logger.Log($"Deserialization failed: {ex.Message}");
            throw; 
        }
    }

    private static Models.Task? ConvertToTaskModel(AppSyncUserTask response)
    {
        Models.Task? task = JsonSerializer.Deserialize<Models.Task>(response.Data);
        if (task is not null)
            task.UpdatedOnDate = response.UpdatedAt;
        return task;
    }
    #endregion
}