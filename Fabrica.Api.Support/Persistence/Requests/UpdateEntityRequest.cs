
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;

namespace Fabrica.Api.Persistence.Requests;

public record UpdateEntityRequest<TDelta>(string Uid, TDelta Delta) :BaseUpdateEntityRequest<TDelta>(Uid, Delta) where TDelta : BaseDelta;