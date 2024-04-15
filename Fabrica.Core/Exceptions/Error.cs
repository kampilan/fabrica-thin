
namespace Fabrica.Exceptions;


public class Ok
{
    
    public static readonly Ok Singleton = new ();

}

public class Error
{

    public static readonly Error Ok = new() {Kind = ErrorKind.None, ErrorCode = "", Explanation = "", Details = [] };

    public ErrorKind Kind { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<EventDetail> Details { get; set; } = null!;

}


public class NotFoundError: Error
{

    public static NotFoundError Create(string explanation)
    {

        var error = new NotFoundError
        {
            Kind = ErrorKind.NotFound,
            ErrorCode = "Not Found",
            Explanation = explanation
        };

        return error;

    }

}


public class NotValidError : Error
{

    public static NotValidError Create(IEnumerable<EventDetail> violations )
    {

        var error = new NotValidError
        {
            Kind = ErrorKind.Predicate,
            ErrorCode = "ValidationFailure",
            Explanation = "Validation errors exist.",
            Details = [..violations]
        };

        return error;

    }

}


public class UnhandledError : Error
{

    public static UnhandledError Create( Exception cause )
    {

        var errorCode = cause.GetType().Name.Replace("Exception", "");
        if (string.IsNullOrWhiteSpace(errorCode))
            errorCode = "Exception";

        var error = new UnhandledError()
        {
            Kind        = ErrorKind.System,
            ErrorCode   = errorCode,
            Explanation = "An unhandled exception was caught. See logs for details."
        };

        return error;

    }

}
