namespace Fabrica.Utilities.Queue;

public class HubQueueMessage() : BaseQueueMessage<HubQueueMessage>( "hub-message" )
{

    public string Tenant
    {
        get => Attributes.TryGetValue("tenant", out var value) ? value : string.Empty;
        set => Attributes["tenant"] = value;
    }

    public string Environment
    {
        get => Attributes.TryGetValue("environment", out var value) ? value : string.Empty;
        set => Attributes["environment"] = value;
    }

    public string Topic
    {
        get => Attributes.TryGetValue("topic", out var value) ? value : string.Empty;
        set => Attributes["topic"] = value;
    }


    public HubQueueMessage WithTenant(string tenant)
    {
        Tenant = tenant;
        return this;
    }

    public HubQueueMessage WithEnvironment(string environment)
    {
        Environment = environment;
        return this;
    }

    public HubQueueMessage WithTopic(string topic)
    {
        Topic = topic;
        return this;
    }



}