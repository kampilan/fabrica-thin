using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Utilities.Text.Json;

public class CompactJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {


        var typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return typeInfo;


        foreach (var prop in typeInfo.Properties)
        {


            if (prop.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
            {

                prop.ShouldSerialize = Should;

                static bool Should(object o, object? value)
                {

                    var result = true;

                    if (value is IEnumerable enumerable)
                    {
                        var e = enumerable.GetEnumerator();
                        using var unknown = e as IDisposable;
                        result = e.MoveNext();
                    }

                    return result;

                }

            }
            else if (prop.PropertyType == typeof(string))
            {

                prop.ShouldSerialize = Should;

                static bool Should(object o, object? value)
                {

                    var isBlank = value is string s && string.IsNullOrWhiteSpace(s);
                    return !isBlank;

                }

            }
            else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long) || prop.PropertyType == typeof(short) || prop.PropertyType == typeof(byte) || prop.PropertyType == typeof(float) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(decimal))
            {

                prop.ShouldSerialize = Should;

                static bool Should(object o, object? value)
                {
                    var isZero = value is 0 || value is 0L || value is short and 0 || value is byte and 0 || value is 0f || value is 0d || value is 0M;
                    return !isZero;

                }

            }
            else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateOnly) || prop.PropertyType == typeof(TimeOnly))
            {

                prop.ShouldSerialize = Should;

                static bool Should(object o, object? value)
                {
                    var isMinimum = (value is DateTime dt && dt == DateTime.MinValue) || (value is DateOnly dto && dto == DateOnly.MinValue) || (value is TimeOnly to && to == TimeOnly.MinValue);
                    return !isMinimum;
                }

            }



        }


        return typeInfo;


    }

}