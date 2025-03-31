using System.Collections;

namespace LightList.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class AppSyncGenericResponse
{
    [JsonPropertyName("errors")]
    public List<AppSyncErrorObject>? Errors { get; set; }
}

public class AppSyncErrorObject
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class AppSyncGetUserTasks
{
    [JsonPropertyName("data")]
    public AppSyncUserTasks Data { get; set; } = new();
}

public class AppSyncUserTasks
{
    [JsonPropertyName("getUserTasks")]
    public List<AppSyncUserTask> UserTasks { get; set; } = new();
}

public class AppSyncUserTask
{
    [JsonPropertyName("ItemId")]
    public string ItemId { get; set; }

    [JsonPropertyName("Data")]
    public string Data { get; set; }

    [JsonPropertyName("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }
}