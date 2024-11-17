
namespace Fabrica.Utilities.Queue;

public class WorkQueueMessage(): BaseQueueMessage<WorkQueueMessage>("work-message")
{

    public string Topic
    {
        get => Attributes.TryGetValue("topic", out var value) ? value : string.Empty;
        set => Attributes["topic"] = value;
    }

    public WorkQueueMessage WithTopic(string topic)
    {
        Topic = topic;
        return this;
    }


}