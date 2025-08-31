using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Fabrica.Aws.DynamoDb.Converters;

public class DynamoDbDataTimeConverter: IPropertyConverter
{

    public object FromEntry(DynamoDBEntry entry)
    {

        var str = entry.AsString();
        var utc = DateTime.Parse(str);
        var local = DateTime.SpecifyKind(utc, DateTimeKind.Local);
        
        return local;
        
    } 

    public DynamoDBEntry ToEntry(object value)
    {
        
        var dt = ((DateTime)value).ToUniversalTime();
        var str = dt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        return str;
        
    }    
    
    
}