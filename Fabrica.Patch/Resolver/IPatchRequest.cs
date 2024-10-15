using Fabrica.Patch.Builder;

namespace Fabrica.Patch.Resolver;

public interface IPatchRequest
{

    void FromPatch(ModelPatch patch);

}