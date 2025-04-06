using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LightList.Messages;

public class TaskSavedMessage : ValueChangedMessage<string>
{
    public TaskSavedMessage(string taskId) : base(taskId)
    {
    }
}