using System.Drawing;
using Autofac;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Patch.Resolver;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using MediatR;

namespace Fabrica.Tests;

public class PatchTestModule : ServiceModule
{

    public PatchTestModule()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();

        maker.UseLocalSwitchSource()
            .WhenMatched("Microsoft", "", Level.Warning, Color.LightGreen)
            .WhenNotMatched(Level.Debug, Color.LightSalmon);

        maker.Build();

    }


    protected override void Load()
    {

        Builder.Register(c =>
            {

                var source = new RequestTypeSource();
                source.AddTypes(typeof(Person).Assembly);

                var comp = new ResolverService(source);

                return comp;

            })
            .As<IRequiresStart>()
            .AsSelf()
            .SingleInstance();


        Builder.AddCorrelation();
        Builder.UseRules();

        Builder.RegisterRequestMediator(this.GetType().Assembly);


    }

}

[Resolve(Target = typeof(Person), Operation = ResolveOperation.Create)]
public class CreatePersonRequest : BaseDeltaRequest, IRequest<Response>
{

}

public class CreatePersonHandler : IRequestHandler<CreatePersonRequest, Response>
{

    public Task<Response> Handle(CreatePersonRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Response.Ok());
    }

}


[Resolve(Target = typeof(Person), Operation = ResolveOperation.Update)]
public class UpdatePersonRequest : BaseDeltaRequest, IRequest<Response>
{

}

public class UpdatePersonHandler : IRequestHandler<UpdatePersonRequest, Response>
{

    public Task<Response> Handle(UpdatePersonRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Response.Ok());
    }

}


[Resolve(Target = typeof(Person), Operation = ResolveOperation.Delete)]
public class DeletePersonRequest : BaseDeleteRequest, IRequest<Response>
{

}

public class DeletePersonHandler : IRequestHandler<DeletePersonRequest, Response>
{

    public Task<Response> Handle(DeletePersonRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Response.Ok());
    }

}


[Resolve(Target = typeof(Address), Operation = ResolveOperation.CreateMember)]
public class CreateAddressRequest : BaseCreateMemberRequest, IRequest<Response>
{

}

public class CreateAddressHandler : IRequestHandler<CreateAddressRequest, Response>
{

    public Task<Response> Handle(CreateAddressRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Response.Ok());

    }

}


[Resolve(Target = typeof(Address), Operation = ResolveOperation.Update)]
public class UpdateAddressRequest : BaseDeltaRequest, IRequest<Response>
{

}

public class UpdateAddressHandler : IRequestHandler<UpdateAddressRequest, Response>
{

    public Task<Response> Handle(UpdateAddressRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Response.Ok());

    }

}


[Resolve(Target = typeof(Address), Operation = ResolveOperation.Delete)]
public class DeleteAddressRequest : BaseDeleteRequest, IRequest<Response>
{

}

public class DeleteAddressHandler : IRequestHandler<DeleteAddressRequest, Response>
{

    public Task<Response> Handle(DeleteAddressRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Response.Ok());
    }

}



[Resolve(Target = typeof(School), Operation = ResolveOperation.Create)]
public class CreateSchoolRequest : BaseDeltaRequest, IRequest<Response>
{

}

public class CreateSchoolHandler : IRequestHandler<CreateSchoolRequest, Response>
{

    public Task<Response> Handle(CreateSchoolRequest request, CancellationToken cancellationToken)
    {

        return Task.FromResult(Response.Ok());
    }

}


[Resolve(Target = typeof(School), Operation = ResolveOperation.Update)]
public class UpdateSchoolRequest : BaseDeltaRequest, IRequest<Response>
{

}

public class UpdateSchoolHandler : IRequestHandler<UpdateSchoolRequest, Response>
{

    public Task<Response> Handle(UpdateSchoolRequest request, CancellationToken cancellationToken)
    {

        return Task.FromResult(Response.Ok());
    }

}


[Resolve(Target = typeof(School), Operation = ResolveOperation.Delete)]
public class DeleteSchoolRequest : BaseDeleteRequest, IRequest<Response>
{

}

public class DeleteSchoolHandler : IRequestHandler<DeleteSchoolRequest, Response>
{

    public Task<Response> Handle(DeleteSchoolRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Response.Ok());
    }

}