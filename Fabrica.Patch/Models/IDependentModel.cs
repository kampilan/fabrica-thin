namespace Fabrica.Patch.Models;

public interface IDependentModel: IMutableModel
{

    void SetParent(object? parent);


}