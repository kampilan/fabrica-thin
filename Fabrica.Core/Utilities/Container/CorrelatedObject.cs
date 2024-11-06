using System.Runtime.CompilerServices;
using Fabrica.Watch;

namespace Fabrica.Utilities.Container;

public abstract class CorrelatedObject(ICorrelation correlation)
{

    protected ICorrelation Correlation { get; } = correlation;

    protected ILogger GetLogger()
    {

        var logger = Correlation.GetLogger(this);

        return logger;

    }

    protected ILogger EnterMethod( [CallerMemberName] string name = "" )
    {

        var logger = Correlation.EnterMethod(GetType(), name);

        return logger;

    }


}