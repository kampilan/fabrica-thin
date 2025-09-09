namespace Fabrica.Identity.Client;

public interface ITokenSource
{

    string Name { get; }
    bool HasExpired { get; }
    Task<string> GetToken();

    Task CheckForRenewal(bool force=false);

}