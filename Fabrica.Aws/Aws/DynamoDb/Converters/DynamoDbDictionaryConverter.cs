using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Fabrica.Aws.DynamoDb.Converters;

public class DynamoDbDictionaryConverter: IPropertyConverter 
{  

    private static readonly JsonSerializerOptions Options = JsonSerializerOptions.Default;
    
    public object FromEntry(DynamoDBEntry entry) 
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(entry.AsString(), Options);
        return dict??[];        
    } 

    public DynamoDBEntry ToEntry(object value)
    {
        var json = JsonSerializer.Serialize(value, Options);
        return new Primitive(json);
    }
    
}