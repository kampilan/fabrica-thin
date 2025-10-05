using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Mapster;
using Mapster.Adapters;

namespace Fabrica.App.Mapping;

public static class TypeAdapterConfigExtensions
{
    public static void EnableJsonObjectMapping(this TypeAdapterConfig config)
    {
        config.Rules.Add(new JsonObjectAdapter().CreateRule());
    }
}

public class JsonObjectAdapter : BaseAdapter
    {
        protected override int Score => -111;   //execute after string (-110)
        protected override bool CheckExplicitMapping => false;

        protected override bool CanMap(PreCompileArgument arg)
        {
            return typeof(JsonObject).IsAssignableFrom(arg.SourceType) ||
                   typeof(JsonObject).IsAssignableFrom(arg.DestinationType);
        }

        protected override Expression CreateExpressionBody(Expression source, Expression destination, CompileArgument arg)
        {
            //source & dest are json, just return reference
            if (arg.SourceType == arg.DestinationType)
                return source;

            //from json
            if (typeof(JsonObject).IsAssignableFrom(arg.SourceType))
            {
                //json.ToObject<T>();
                var toObject = (from method in arg.SourceType.GetMethods()
                                where method.Name == nameof(JsonObject.AsObject) &&
                                      method.IsGenericMethod &&
                                      method.GetParameters().Length == 0
                                select method).First().MakeGenericMethod(arg.DestinationType);
                return Expression.Call(source, toObject);
            }

            else //to json
            {
                //JToken.FromObject(source);
                var fromObject = typeof(JsonObject).GetMethod(nameof(JsonObject.AsObject), [typeof(object)]);
                Expression exp = Expression.Call(fromObject, source);
                if (arg.DestinationType != typeof(JsonObject))
                    exp = Expression.Convert(exp, arg.DestinationType);
                return exp;
            }
        }

        protected override Expression CreateBlockExpression(Expression source, Expression destination, CompileArgument arg)
        {
            throw new System.NotImplementedException();
        }

        protected override Expression CreateInlineExpression(Expression source, CompileArgument arg)
        {
            throw new System.NotImplementedException();
        }
    }