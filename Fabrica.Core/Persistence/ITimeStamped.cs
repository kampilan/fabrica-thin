namespace Fabrica.Persistence;

public interface ITimeStamped
{

    DateTime CreatedTime { get; set; }
    DateTime LastModifiedTime { get; set; }

}