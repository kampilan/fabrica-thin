
// ReSharper disable UnusedTypeParameter

using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using MediatR;

namespace Fabrica.Api.Persistence.Requests;

public abstract record BaseCreateMemberEntityRequest<TParent, TDelta>(string ParentUid, TDelta Delta) : IRequest<Response> where TParent : class where TDelta : BaseDelta;