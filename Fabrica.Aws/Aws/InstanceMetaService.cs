using Amazon.Util;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

// ReSharper disable MemberCanBePrivate.Global

namespace Fabrica.Aws;

public interface IInstanceMetadata
{
    
    bool IsRunningOnEc2 { get; }
    
    string InstanceId { get; }
    string Region { get; }
    string UserData { get; }    
    
}


internal class InstanceMetaService: IRequiresStart, IInstanceMetadata
{

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(1);
    
    public string DefaultInstanceId { get; init; } = string.Empty;
    public string DefaultRegion { get; init; } = string.Empty;    
    public string DefaultUserData { get; init; } = string.Empty;    
    
    
    private bool _isRunningOnEc2 = true;
    
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
                    _isRunningOnEc2 = false;

                if( !_isRunningOnEc2 )
                {
                    using var innerLogger = this.GetLogger();
                    innerLogger.Warning("NOT Running on EC2 Instance.");                
                }
                
            }
            catch
            {
                using var innerLogger = this.GetLogger();
                innerLogger.Warning("NOT Running on EC2 Instance.");
                _isRunningOnEc2 = false;
            }
            
        }, ct);

        
        // *************************************************
        logger.Debug("Attempting to wait for Meta or Delay Task to finish");
        var delayTask = Task.Delay(Timeout, ct);

        var index = Task.WaitAny( metaTask, delayTask );
        if( index == 1)
            _isRunningOnEc2 = false;

        logger.Inspect(nameof(index), index);

        
        tokenSource.Cancel();        

        
        // *************************************************
        return Task.CompletedTask;
        
    }

    public bool IsRunningOnEc2 => _isRunningOnEc2;
    
    public string InstanceId => _isRunningOnEc2 ? EC2InstanceMetadata.InstanceId : DefaultInstanceId;
    public string Region     => _isRunningOnEc2 ? EC2InstanceMetadata.Region.SystemName : DefaultRegion;
    public string UserData   => _isRunningOnEc2 ? EC2InstanceMetadata.UserData: DefaultUserData;     

    bool IInstanceMetadata.IsRunningOnEc2 => IsRunningOnEc2;
    string IInstanceMetadata.InstanceId => InstanceId;
    string IInstanceMetadata.Region     => Region;
    string IInstanceMetadata.UserData   => UserData;
    
}