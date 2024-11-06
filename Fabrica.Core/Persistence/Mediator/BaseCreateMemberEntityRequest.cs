
// ReSharper disable UnusedTypeParameter

using Fabrica.Models;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public abstract record BaseCreateMemberEntityRequest<TParent, TDelta>(string ParentUid, TDelta Delta) : IRequest<Response> where TParent : class where TDelta : BaseDelta;