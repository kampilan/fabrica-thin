using System;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Fabrica.Aws.DynamoDb.Converters;

/// <summary>
/// A custom property converter for DynamoDB, responsible for serializing and
/// deserializing Enum objects to and from their string representation.
/// </summary>
public class DynamoDbEnumConverter<TEnum> : IPropertyConverter where TEnum : struct, Enum
{

    public object FromEntry(DynamoDBEntry entry)
    {

        var value = entry.AsString();

        if (string.IsNullOrWhiteSpace(value))
            return default(TEnum);

        return Enum.Parse<TEnum>(value, true);
        
    }

    public DynamoDBEntry ToEntry(object value)
    {
        return new Primitive(value.ToString()!);
    }
    
    
}