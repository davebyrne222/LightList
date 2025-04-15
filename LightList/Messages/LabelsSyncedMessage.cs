using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LightList.Messages;

public class LabelsSyncedMessage : ValueChangedMessage<bool>
{
    public LabelsSyncedMessage(bool _) : base(_)
    {
        Console.WriteLine("LabelsSyncedMessage triggered");
    }
}