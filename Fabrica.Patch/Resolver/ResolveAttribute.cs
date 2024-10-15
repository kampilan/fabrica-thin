namespace Fabrica.Patch.Resolver;


public enum ResolveOperation
{
    None,
    Create,
    CreateMember,
    Update,
    Delete
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ResolveAttribute: Attribute
{
    public string Alias { get; set; } = string.Empty;
    public Type Target { get; set; } = null!;
    public ResolveOperation Operation { get; set; } = ResolveOperation.None;

}