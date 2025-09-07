using Amazon.Util;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

// ReSharper disable MemberCanBePrivate.Global

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
    
    public string DefaultInstanceId { get; set; } = string.Empty;
    public string DefaultRegion { get; set; } = string.Empty;    
    public string DefaultUserData { get; set; } = string.Empty;    
    
    
    private bool _useInstanceMetadata = true;
    
    public Task Start()
    {

        using var logger = this.EnterMethod();

        
        var tokenSource = new CancellationTokenSource();
        CancellationToken ct = tokenSource.Token;        
        
        var metaTask = Task.Run(() =>
        {
            try
            {

                var instanceId = EC2InstanceMetadata.InstanceId;
                if( string.IsNullOrWhiteSpace(instanceId) )
                    _useInstanceMetadata = false;

                if( !_useInstanceMetadata )
                {
                    using var innerLogger = this.GetLogger();
                    innerLogger.Warning("NOT Running on EC2 Instance.");                
                }
                
            }
            catch
            {
                using var innerLogger = this.GetLogger();
                innerLogger.Warning("NOT Running on EC2 Instance.");
                _useInstanceMetadata = false;
            }
            
        }, ct);

        
        // *************************************************
        logger.Debug("Attempting to wait for Meta or Delay Task to finish");
        var delayTask = Task.Delay(Timeout, ct);

        var index = Task.WaitAny( metaTask, delayTask );
        if( index == 1)
            _useInstanceMetadata = false;

        logger.Inspect(nameof(index), index);

        
        tokenSource.Cancel();        

        
        // *************************************************
        return Task.CompletedTask;
        
    }

    public bool UseInstanceMetadata => _useInstanceMetadata;
    
    public string InstanceId => _useInstanceMetadata ? EC2InstanceMetadata.InstanceId : DefaultInstanceId;
    public string Region     => _useInstanceMetadata ? EC2InstanceMetadata.Region.SystemName : DefaultRegion;
    public string UserData   => _useInstanceMetadata ? EC2InstanceMetadata.UserData: DefaultUserData;     

    bool IInstanceMetadata.UseInstanceMetadata => UseInstanceMetadata;
    string IInstanceMetadata.InstanceId => InstanceId;
    string IInstanceMetadata.Region     => Region;
    string IInstanceMetadata.UserData   => UserData;
    
}