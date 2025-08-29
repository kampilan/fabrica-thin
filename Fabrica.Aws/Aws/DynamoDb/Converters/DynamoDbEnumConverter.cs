using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Fabrica.Aws.DynamoDb.Converters;

public class DynamoDbEnumConverter <TEnum>: IPropertyConverter where TEnum: struct
{  

    public object FromEntry(DynamoDBEntry entry) 
    {
        return Enum.Parse<TEnum>(entry.AsString());
    } 

    public DynamoDBEntry ToEntry(object value) 
    {
        return value.ToString();
    }
    
}