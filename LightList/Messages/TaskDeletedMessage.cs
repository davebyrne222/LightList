using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LightList.Messages;

public class TaskDeletedMessage: ValueChangedMessage<string>
{
    public TaskDeletedMessage(string taskId) : base(taskId) { }
}
