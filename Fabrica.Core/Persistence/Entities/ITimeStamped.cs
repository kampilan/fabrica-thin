namespace Fabrica.Persistence.Entities;

public interface ITimeStamped
{

    DateTime CreatedTime { get; set; }
    DateTime LastModifiedTime { get; set; }

}