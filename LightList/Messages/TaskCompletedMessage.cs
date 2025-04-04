using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LightList.Messages;

public class TaskCompletedMessage : ValueChangedMessage<string>
{
    public TaskCompletedMessage(string taskId) : base(taskId)
    {
    }
}