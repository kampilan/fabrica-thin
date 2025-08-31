using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Fabrica.Aws.DynamoDb.Converters;

/// <summary>
/// A custom property converter for DynamoDB, responsible for serializing and
/// deserializing Dictionary{string, string}> objects to and from the DynamoDBEntry in JSON format.
/// </summary>
public class DynamoDbDictionaryConverter: IPropertyConverter 
{  

    private static readonly JsonSerializerOptions Options = JsonSerializerOptions.Default;
    
    public object FromEntry(DynamoDBEntry entry) 
    {

        var source = entry.AsString();
        if( string.IsNullOrWhiteSpace(source) )
            return new Dictionary<string, string>();
        
        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(entry.AsString(), Options);
        return dict??[];        
    } 

    public DynamoDBEntry ToEntry(object value)
    {

        if( value is Dictionary<string, string> { Count: 0 })
            return new Primitive("");
        
        var json = JsonSerializer.Serialize(value, Options);
        return new Primitive(json);
        
    }
    
}