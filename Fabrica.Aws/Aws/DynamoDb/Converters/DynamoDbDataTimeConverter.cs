using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Fabrica.Aws.DynamoDb.Converters;

public class DynamoDbDataTimeConverter: IPropertyConverter
{

    public object FromEntry(DynamoDBEntry entry)
    {

        var str = entry.AsString();

        if (string.IsNullOrWhiteSpace(str))
            return default(DateTime);

        if( DateTime.TryParse( str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var utc) )
            return utc.ToLocalTime();

        // Fallback parse if the stored string is not clearly marked as UTC
        var parsed = DateTime.Parse(str, CultureInfo.InvariantCulture, DateTimeStyles.None);
        if (parsed.Kind == DateTimeKind.Unspecified)
            parsed = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

        return parsed.ToLocalTime();

        
    } 

    public DynamoDBEntry ToEntry(object value)
    {

        var dt = Convert.ToDateTime(value, CultureInfo.InvariantCulture);

        if (dt.Kind == DateTimeKind.Unspecified)
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);

        var utc = dt.ToUniversalTime();
        var str = utc.ToString("o", CultureInfo.InvariantCulture); // e.g., 2025-08-31T13:45:30.1234567Z

        return str;
        
    }    
    
    
}