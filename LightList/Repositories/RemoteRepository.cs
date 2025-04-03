using System.Text;
using System.Text.Json;
using LightList.Models;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public class RemoteRepository : IRemoteRepository
{
    private static readonly HttpClient Client = new();

    public async Task<List<Models.Task?>> GetUserTasks(AuthTokens accessToken, DateTime? lastSyncDate)
    {
        Logger.Log($"Retrieving remote tasks (lastSyncDate: {(lastSyncDate.HasValue ? lastSyncDate : "NA")})");
        
        string syncDate = lastSyncDate.HasValue ? AwsDatetimeConverter(lastSyncDate.Value) : "";
        
        var query = 
            $$"""
              query { getUserTasks(
                UserId: "{{accessToken.UserId}}"
                {{(lastSyncDate.HasValue ? $", UpdatedOnOrAfter: \"{syncDate}\"" : "")}}
              ) {
                ItemId,
                Data,
                UpdatedAt
              } }
              """;
        
        string result = await ExecuteQuery(accessToken.AccessToken, query);
        return DeserializeUserTasks(result);
    }

    public async Task PushUserTask(AuthTokens accessToken, Models.Task task)
    {
        Logger.Log($"Pushing task to remote (task: {task.Id})");
        
        var escapedData = JsonSerializer.Serialize(task)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
            
        var query = 
            $$"""
              mutation { saveUserTask(
                UserId: "{{accessToken.UserId}}",
                ItemId: "{{task.Uid}}",
                Data: "{{escapedData}}",
                UpdatedAt: "{{AwsDatetimeConverter(DateTime.UtcNow)}}"
              ) {
                UserId,
                ItemId,
                Data,
                UpdatedAt
              } }
              """; 
        
        await ExecuteQuery(accessToken.AccessToken, query);
        
        Logger.Log($"Change pushed successfully");
    }
    
    #region Utils

    private async Task<string> ExecuteQuery(string accessToken, string query)
    {
        Logger.Log($"Executing query: \n{query}");

        var request = new HttpRequestMessage(HttpMethod.Post, Constants.AppSyncEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json")
        };
        
        request.Headers.Add("Authorization", accessToken);
        
        var response = await Client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var msg = $"Query failed: {response.StatusCode} - {response.ReasonPhrase}";
            Logger.Log(msg);
            throw new HttpRequestException(msg);
        }
        
        Logger.Log($"Query succeeded");

        return CheckAppSyncResponse(await response.Content.ReadAsStringAsync());
    }
    
    private static string CheckAppSyncResponse(string contentString)
    {
        Logger.Log("Checking app sync response for errors");

        AppSyncGenericResponse? contentJson;
        try
        {
            contentJson = JsonSerializer.Deserialize<AppSyncGenericResponse>(contentString);
        }
        catch (JsonException ex)
        {
            Logger.Log($"De-serialisation failed: {ex.Message}");
            throw;
        }

        if (contentJson != null && contentJson.Errors != null)
        {
            string errors = String.Empty;
            foreach (var error in contentJson.Errors)
            {
                Logger.Log($"AppSync error: {error.Message}");
                errors += error.Message + Environment.NewLine;
            }
            throw new ArgumentException(errors);
        }
        Logger.Log("No errors found");
        
        return contentString;
    }

    private static List<Models.Task?> DeserializeUserTasks(string response)
    {
        Logger.Log("Deserializing response");
        
        if (string.IsNullOrEmpty(response))
            throw new ArgumentNullException(nameof(response));
        
        try
        {
            var result = JsonSerializer.Deserialize<AppSyncGetUserTasks>(response);

            if (result == null)
                throw new NullReferenceException("Deserializing failed: Response is null");

            Logger.Log($"Successfully deserialized response: {result.Data.UserTasks?.Count ?? 0} user tasks retrieved");

            if (result.Data.UserTasks == null)
                return [];

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
        Logger.Log("Converting task response to model");
        var task = JsonSerializer.Deserialize<Models.Task>(response.Data);
        if (task is not null)
            task.UpdatedOnDate = response.UpdatedAt;
        return task;
    }

    private static string AwsDatetimeConverter(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }
    #endregion
}