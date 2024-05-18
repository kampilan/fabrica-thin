using Fabrica.Patch.Models;
using Fabrica.Utilities.Types;

namespace Fabrica.Tests;

public class School: BaseReferenceModel
{
    public override string GetUid()
    {
        return Uid;
    }

    public string Uid { get; private set; } = Ulid.NewUlid();

    public string Name { get; set; } = string.Empty;



}