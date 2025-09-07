using Amazon.Util;
using Fabrica.Utilities.Container;

namespace Fabrica.Aws;

public interface IInstanceMetadata
{

    bool UseInstanceMetadata { get; }
    
    string InstanceId { get; }
    string Region { get; }
    string UserData { get; }    
    
}


internal class InstanceMetaService: IRequiresStart, IInstanceMetadata
{

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);
    
    private bool _useInstanceMetadata;
    
    public Task Start()
    {

        var metaTask = Task.Run(() =>
        {
            try
            {
                var id = EC2InstanceMetadata.InstanceId;
            }
            catch
            {
                // ignored
            }
            
        });

        var delayTask = Task.Delay(Timeout);
        
        var index = Task.WaitAny( metaTask, delayTask);
        _useInstanceMetadata = index == 0;
        

        return Task.CompletedTask;
        
    }

    public bool UseInstanceMetadata => _useInstanceMetadata;
    
    public string InstanceId => _useInstanceMetadata ? EC2InstanceMetadata.InstanceId : "";
    public string Region     => _useInstanceMetadata ? EC2InstanceMetadata.Region.SystemName : "";
    public string UserData   => _useInstanceMetadata ? EC2InstanceMetadata.UserData: "";     

    bool IInstanceMetadata.UseInstanceMetadata => UseInstanceMetadata;
    string IInstanceMetadata.InstanceId => InstanceId;
    string IInstanceMetadata.Region     => Region;
    string IInstanceMetadata.UserData   => UserData;
    
}