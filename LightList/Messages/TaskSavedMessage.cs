using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LightList.Messages;

public class TaskSavedMessage: ValueChangedMessage<int>
{
    public TaskSavedMessage(int taskId) : base(taskId) { }
}