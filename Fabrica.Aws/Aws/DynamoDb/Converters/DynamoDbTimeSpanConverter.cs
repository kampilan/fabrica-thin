using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Fabrica.Aws.DynamoDb.Converters;

/// <summary>
/// A custom property converter for DynamoDB, responsible for serializing and
/// deserializing TimeSpan objects to and from an integer representing total seconds.
/// </summary>
public class DynamoDbTimeSpanConverter : IPropertyConverter
{

    public object FromEntry(DynamoDBEntry entry)
    {
        var str = entry.AsString();
        return !int.TryParse(str, out var seconds) ? TimeSpan.Zero : TimeSpan.FromSeconds(seconds);
    }

    public DynamoDBEntry ToEntry(object value)
    {

        
        if (value is not TimeSpan ts)
            return 0;

        var totalSeconds = Convert.ToInt32(Math.Round(ts.TotalSeconds));

        return totalSeconds;
        
    }
    
    
}