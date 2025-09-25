
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;

namespace Fabrica.Api.Persistence.Requests;

public record CreateMemberEntityRequest<TParent, TDelta>( string ParentUid, TDelta Delta ) : BaseCreateMemberEntityRequest<TParent,TDelta>( ParentUid, Delta ) where TParent : class where TDelta : BaseDelta;