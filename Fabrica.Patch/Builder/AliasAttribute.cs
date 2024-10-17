namespace Fabrica.Patch.Builder;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AliasAttribute(string alias) : Attribute
{
    public string Alias => alias;

}
