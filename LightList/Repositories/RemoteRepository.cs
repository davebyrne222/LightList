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

    #region Public methods - Tasks
    
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

        string result = await ExecuteQuery(accessToken.AccessToken, query);
        var appSyncObj = DeserializeAppSyncResponse<AppSyncGetUserTasks>(result);

        if (appSyncObj.Data.UserTasks == null)
            return [];
        
        return appSyncObj.Data.UserTasks.Select(
            ConvertToModel<Models.Task, AppSyncUserTask>
        ).ToList();
    }

    #endregion

    #region Public methods - Labels

    public async Task PushUserLabel(AuthTokens accessToken, Models.Label label)
    {
        _logger.Debug($"Pushing label to remote (label: {label.Name})");

        var escapedData = JsonSerializer.Serialize(label)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");

        var query =
            $$"""
              mutation { saveUserLabel(
                UserId: "{{accessToken.UserId}}",
                Name: "{{label.Name}}",
                Data: "{{escapedData}}",
                UpdatedAt: "{{AwsDatetimeConverter(DateTime.UtcNow)}}"
              ) {
                UserId,
                Name,
                Data,
                UpdatedAt
              } }
              """;

        await ExecuteQuery(accessToken.AccessToken, query);

        _logger.Debug("Change pushed successfully");
    }
    
    public async Task<List<Models.Label?>> GetUserLabels(AuthTokens accessToken, DateTime? lastSyncDate)
    {
        _logger.Debug($"Retrieving remote labels (lastSyncDate: {(lastSyncDate.HasValue ? lastSyncDate : "NA")})");

        var syncDate = lastSyncDate.HasValue ? AwsDatetimeConverter(lastSyncDate.Value) : "";

        var query =
            $$"""
              query { getUserLabels(     
                UserId: "{{accessToken.UserId}}"
                {{(lastSyncDate.HasValue ? $", UpdatedOnOrAfter: \"{syncDate}\"" : "")}}
              ) {
                Name,
                Data,
                UpdatedAt
              } }
              """;

        string result = await ExecuteQuery(accessToken.AccessToken, query);
        var appSyncObj = DeserializeAppSyncResponse<AppSyncGetUserLabels>(result);

        if (appSyncObj.Data.UserLabels == null)
            return [];
        
        return appSyncObj.Data.UserLabels.Select(
            ConvertToModel<Models.Label, AppSyncUserLabel>
        ).ToList();
    }

    #endregion
    
    #region Utils

    private async Task<string> ExecuteQuery(string accessToken, string query)
    {
        _logger.Debug("Executing query");

        query = query.Replace("\n", "");

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

        _logger.Debug($"Query succeeded (response code: {response.StatusCode})");

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
    
    private T DeserializeAppSyncResponse<T>(string response)
    {
        _logger.Debug("Deserializing response");

        if (string.IsNullOrEmpty(response))
            throw new ArgumentNullException(nameof(response));

        try
        {
            var result = JsonSerializer.Deserialize<T>(response);

            if (result == null)
                throw new NullReferenceException("Deserializing failed: Response is null");

            _logger.Debug("Successfully deserialized response");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"Deserialization failed: {ex.Message}");
            throw;
        }
    }
    
    private TModel? ConvertToModel<TModel, TWrapper>(TWrapper response)
        where TModel : class
        where TWrapper : class
    {
        _logger.Debug($"Converting {typeof(TModel).Name} response to model");

        var dataProp = typeof(TWrapper).GetProperty("Data");
        var updatedAtProp = typeof(TWrapper).GetProperty("UpdatedAt");

        if (dataProp == null || updatedAtProp == null)
            throw new InvalidOperationException($"Wrapper {typeof(TWrapper).Name} must have Data and UpdatedAt properties.");

        var dataJson = dataProp.GetValue(response) as string;
        var updatedAt = (DateTime) updatedAtProp.GetValue(response)!;

        if (string.IsNullOrEmpty(dataJson))
            return null;

        var model = JsonSerializer.Deserialize<TModel>(dataJson);
        if (model == null)
            return null;

        // Set UpdatedAt on model
        var updatedAtOnModel = typeof(TModel).GetProperty("UpdatedAt");
        if (updatedAtOnModel != null && updatedAtOnModel.CanWrite)
        {
            updatedAtOnModel.SetValue(model, updatedAt);
        }

        return model;
    }

    private static string AwsDatetimeConverter(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    #endregion
}