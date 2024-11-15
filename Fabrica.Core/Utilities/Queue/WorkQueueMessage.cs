using System.Text.Json.Nodes;
using System.Text;

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


    private JsonNode? _bodyNode;

    public JsonNode? BodyNode
    {
        get
        {

            if (_bodyNode is not null)
                return _bodyNode;

            if (!ContentType.StartsWith("application/json"))
                return null;

            var json = Encoding.UTF8.GetString(Content);
            _bodyNode = JsonNode.Parse(json);

            return _bodyNode;

        }

    }


}