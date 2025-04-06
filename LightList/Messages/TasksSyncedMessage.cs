using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LightList.Messages;

public class TasksSyncedMessage: ValueChangedMessage<bool>
{
    public TasksSyncedMessage(bool _) : base(_)
    {
        Console.WriteLine("TasksSyncedMessage triggered");
    }
}