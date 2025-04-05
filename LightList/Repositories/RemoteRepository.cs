using System.Text;
using System.Text.Json;
using LightList.Models;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public class RemoteRepository : IRemoteRepository
{
    private static readonly HttpClient Client = new();
    private readonly ILogger _logger;

    public RemoteRepository(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<List<Models.Task?>> GetUserTasks(AuthTokens accessToken, DateTime? lastSyncDate)
    {
        _logger.Debug($"Retrieving remote tasks (lastSyncDate: {(lastSyncDate.HasValue ? lastSyncDate : "NA")})");

        var syncDate = lastSyncDate.HasValue ? AwsDatetimeConverter(lastSyncDate.Value) : "";

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

        var result = await ExecuteQuery(accessToken.AccessToken, query);
        return DeserializeUserTasks(result);
    }

    public async Task PushUserTask(AuthTokens accessToken, Models.Task task)
    {
        _logger.Debug($"Pushing task to remote (task: {task.Id})");

        var escapedData = JsonSerializer.Serialize(task)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");

        var query =
            $$"""
              mutation { saveUserTask(
                UserId: "{{accessToken.UserId}}",
                ItemId: "{{task.Id}}",
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

        _logger.Debug("Change pushed successfully");
    }

    #region Utils

    private async Task<string> ExecuteQuery(string accessToken, string query)
    {
        _logger.Debug("Executing query");

        var request = new HttpRequestMessage(HttpMethod.Post, Constants.AppSyncEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json")
        };

        request.Headers.Add("Authorization", accessToken);

        var response = await Client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var msg = $"Query failed: {response.StatusCode} - {response.ReasonPhrase}";
            _logger.Debug(msg);
            throw new HttpRequestException(msg);
        }

        _logger.Debug("Query succeeded");

        return CheckAppSyncResponse(await response.Content.ReadAsStringAsync());
    }

    private string CheckAppSyncResponse(string contentString)
    {
        _logger.Debug("Checking app sync response for errors");

        AppSyncGenericResponse? contentJson;
        try
        {
            contentJson = JsonSerializer.Deserialize<AppSyncGenericResponse>(contentString);
        }
        catch (JsonException ex)
        {
            _logger.Error($"De-serialisation failed: {ex.Message}");
            throw;
        }

        if (contentJson != null && contentJson.Errors != null)
        {
            var errors = string.Empty;
            foreach (var error in contentJson.Errors)
            {
                _logger.Debug($"AppSync error: {error.Message}");
                errors += error.Message + Environment.NewLine;
            }

            throw new ArgumentException(errors);
        }

        _logger.Debug("No errors found");

        return contentString;
    }

    private List<Models.Task?> DeserializeUserTasks(string response)
    {
        _logger.Debug("Deserializing response");

        if (string.IsNullOrEmpty(response))
            throw new ArgumentNullException(nameof(response));

        try
        {
            var result = JsonSerializer.Deserialize<AppSyncGetUserTasks>(response);

            if (result == null)
                throw new NullReferenceException("Deserializing failed: Response is null");

            _logger.Debug(
                $"Successfully deserialized response: {result.Data.UserTasks?.Count ?? 0} user tasks retrieved");

            if (result.Data.UserTasks == null)
                return [];

            return result.Data.UserTasks.Select(
                ConvertToTaskModel
            ).ToList();
        }
        catch (Exception ex)
        {
            _logger.Error($"Deserialization failed: {ex.Message}");
            throw;
        }
    }

    private Models.Task? ConvertToTaskModel(AppSyncUserTask response)
    {
        _logger.Debug("Converting task response to model");
        var task = JsonSerializer.Deserialize<Models.Task>(response.Data);
        if (task is not null)
            task.UpdatedAt = response.UpdatedAt;
        return task;
    }

    private static string AwsDatetimeConverter(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    #endregion
}