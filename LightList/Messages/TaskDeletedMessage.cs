using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LightList.Messages;

public class TaskDeletedMessage: ValueChangedMessage<int>
{
    public TaskDeletedMessage(int taskId) : base(taskId) { }
}
