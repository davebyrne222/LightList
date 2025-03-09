using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LightList.Messages;

public class TaskCompletedMessage: ValueChangedMessage<int>
{
    public TaskCompletedMessage(int taskId) : base(taskId) { }
}