using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Fabrica.Aws.DynamoDb.Converters;

public class DynamoDbTimeSpanConverter: IPropertyConverter 
{  

    public object FromEntry(DynamoDBEntry entry) 
    {
        var seconds = entry.AsInt();
        return TimeSpan.FromSeconds(seconds);
    } 

    public DynamoDBEntry ToEntry(object value)
    {

        var secs = 0;
        if( value is TimeSpan ts )
            secs = Convert.ToInt32(ts.TotalSeconds);  

        return secs;
        
    }
    
}