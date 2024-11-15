using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Fabrica.Exceptions;
using Fabrica.Identity;
using Fabrica.Utilities.Types;

namespace Fabrica.Utilities.Queue;

public class QueueMessageException : Exception
{

    public QueueMessageException(string message) : base(message)
    {
    }

    public QueueMessageException(string message, Exception inner) : base(message, inner)
    {
    }

}

public abstract class BaseQueueMessage<TDecedent>(string type) where TDecedent : BaseQueueMessage<TDecedent>, new()
{

    public static TDecedent Create()
    {
        return new TDecedent();
    }


    public static TDecedent Load( string source, string signingKey, string hash )
    {

        var key = Convert.FromBase64String(signingKey);
        var hmac = new HMACSHA256(key);

        var given   = Convert.FromBase64String(hash);
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(source));

        if( given.Length != computed.Length)
            throw new QueueMessageException("Signature Mismatch");

        var matched = true;
        for (var i = 0; i < given.Length; i++)
        {
            if (given[i] != computed[i])
                matched = false;
        }

        if( !matched )
            throw new QueueMessageException("Signature Mismatch");


        var target = Load(source);


        return target;


    }


    public static TDecedent Load( string source )
    {

        var target = new TDecedent();

        using var tw = new StringReader(source);
        var doc = XDocument.Load(tw);
        var root = doc.Root;

        if (root is null)
            return target;


        var type = root.Attribute("type")?.Value;
        if( type != target.Type )
            throw new QueueMessageException("Type Mismatch");


        foreach (var attr in root.Attributes())
        {

            switch (attr.Name.LocalName)
            {

                case "type":
                    target.Type = attr.Value;
                    break;

                case "message-id":
                    target.MessageId = attr.Value;
                    break;

                case "content-type":
                    target.ContentType = attr.Value;
                    break;

                default:
                    target.Attributes[attr.Name.LocalName] = attr.Value;
                    break;


            }

        }


        var claimsElement = root.Element(nameof(Claims).ToLowerInvariant());
        if( claimsElement != null )
        {

            var claims = new ClaimSetModel();
            foreach (var attr in claimsElement.Attributes())
            {

                switch (attr.Name.LocalName)
                {
                    case "subject":
                        claims.Subject = attr.Value;
                        break;
                    case "username":
                        claims.UserName = attr.Value;
                        break;
                    case "name":
                        claims.Name = attr.Value;
                        break;
                    case "roles":
                        var roles = attr.Value.Split(',');
                        claims.Roles = [..roles]; 
                        break;

                }

            }

            target.Claims = claims;


        }

        var content = root.Element(nameof(Content).ToLowerInvariant());
        if (content is {FirstNode: XCData cd})
            target.Content = Encoding.UTF8.GetBytes(cd.Value);


        return target;

    }


    public string Type { get; private set; } = type;

    public string MessageId { get; protected set; } = Ulid.NewUlid();

    public string ContentType { get; protected set; } = string.Empty;


    public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

    public TDecedent WithAttribute(string key, string value)
    {
        Attributes[key] = value;
        return (TDecedent)this;
    }

    public TDecedent WithAttribute(KeyValuePair<string, string> pair)
    {

        Attributes[pair.Key] = pair.Value;
        return (TDecedent)this;
    }

    public TDecedent WithAttributes(IEnumerable<KeyValuePair<string, string>> pairs)
    {

        foreach (var pair in pairs)
            Attributes[pair.Key] = pair.Value;

        return (TDecedent)this;

    }





    protected byte[] Content { get; set; } = [];



    public string Body => Encoding.UTF8.GetString(Content);

    private JsonNode? _bodyNode;
    public JsonNode? BodyNode
    {
        get
        {

            if (_bodyNode is not null)
                return _bodyNode;

            if (!ContentType.StartsWith("application/json"))
                return null;

            _bodyNode = JsonNode.Parse(Body);

            return _bodyNode;

        }

    }


    public TDecedent WithBody( string contentType, string body)
    {

        ContentType = contentType;

        Content = Encoding.UTF8.GetBytes(body);

        return (TDecedent)this;

    }


    public TDecedent WithJson( string body )
    {

        ContentType = "application/json";

        Content = Encoding.UTF8.GetBytes(body);

        return (TDecedent)this;

    }


    public TDecedent WithJson( JsonNode body )
    {

        ContentType = "application/json";

        Content = Encoding.UTF8.GetBytes(body.ToJsonString());

        return (TDecedent)this;

    }

    public TDecedent WithJson<T>( T body, JsonSerializerOptions? options=null ) where T: class
    {

        ContentType = "application/json";

        var json = JsonSerializer.Serialize(body, options);

        Content = Encoding.UTF8.GetBytes(json);

        return (TDecedent)this;

    }


    public IClaimSet Claims { get; set; } = new ClaimSetModel();


    public TDecedent WithClaims( IClaimSet claims )
    {
        Claims = claims;
        return (TDecedent)this;
    }

    public TDecedent WithClaims( IPrincipal? principal )
    {

        if( principal is not null && principal.Identity is ClaimsIdentity ci )
            Claims = ci.ToPayload();

        return (TDecedent)this;

    }


    public TDecedent WithClaims( string subject, string username, string name, IEnumerable<string> roles)
    {

        var set = new ClaimSetModel
        {
            Subject  = subject,
            UserName = username,
            Name     = name,
            Roles    = [..roles]
        };

        Claims = set;

        return (TDecedent)this;


    }


    public T? FromJson<T>( JsonSerializerOptions? options = null) where T : class
    {

        if (ContentType != "application/json")
            return null;

        var json = Encoding.UTF8.GetString(Content);

        var obj = JsonSerializer.Deserialize<T>(json, options);

        return obj;

    }


    public (string body, string hash) Save( string signingKey )
    {

        var xml = Save();

        var key = Convert.FromBase64String(signingKey);
        var hmac = new HMACSHA256(key);

        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(xml));
        var base64 = Convert.ToBase64String(hash);
        
        return (xml,base64);

    }



    public string Save()
    {

        var root = new XElement("envelope");
        
        root.Add(new XAttribute("type", Type));
        root.Add(new XAttribute("message-id", MessageId));
        root.Add(new XAttribute("content-type", ContentType));

        foreach( var pair in Attributes)
            root.Add(new XAttribute(pair.Key, pair.Value));


        var claimsElement = new XElement("claims");

        if( !string.IsNullOrWhiteSpace(Claims.Subject))
            claimsElement.Add(new XAttribute("subject", Claims.Subject));

        if( !string.IsNullOrWhiteSpace(Claims.UserName))
            claimsElement.Add(new XAttribute("username", Claims.UserName));

        if( !string.IsNullOrWhiteSpace(Claims.Name))
            claimsElement.Add(new XAttribute("name", Claims.Name));

        if( Claims.Roles.Any())
            claimsElement.Add(new XAttribute("roles", string.Join(',', Claims.Roles)));

        root.Add(claimsElement);



        if( Content is {Length: > 0} )
        {

            var body = new XElement("content");
            body.Add(new XCData(Encoding.UTF8.GetString(Content)));

            root.Add(body);

        }            


        using var tw = new StringWriter();
        var doc = new XDocument(root);
        doc.Save(tw);
        tw.Flush();

        return tw.ToString();

    }


}