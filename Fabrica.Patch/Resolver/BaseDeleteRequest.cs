using Fabrica.Patch.Builder;

namespace Fabrica.Patch.Resolver;

public class BaseDeleteRequest: BaseEntityRequest, IPatchRequest
{

    public string Uid { get; set; } = "";


    public void FromPatch(ModelPatch patch)
    {

        if (patch == null) throw new ArgumentNullException(nameof(patch));

        Uid = patch.Uid;

    }



}