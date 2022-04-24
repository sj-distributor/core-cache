using System.Dynamic;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using JsonException = Newtonsoft.Json.JsonException;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Net6Test.Tools;

public class CustomerJsonConverter : CustomCreationConverter<IDictionary<string, object>>
{
    public override IDictionary<string, object> Create(Type objectType)
    {
        return new Dictionary<string, object>();
    }

    public override bool CanConvert(Type objectType)
    {
        // in addition to handling IDictionary<string, object>
        // we want to handle the deserialization of dict value
        // which is of type object
        return objectType == typeof(object) || base.CanConvert(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject
            || reader.TokenType == JsonToken.Null)
            return base.ReadJson(reader, objectType, existingValue, serializer);

        if (reader.TokenType == JsonToken.StartArray)
        {
            var list = new List<object>();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    default:
                        list.Add(base.ReadJson( reader, typeof(object), existingValue, serializer));
                        break;
                    case JsonToken.EndArray:
                        return list;
                }
            }
            throw new JsonException();
        }

        // if the next token is not an object
        // then fall back on standard deserializer (strings, numbers etc.)
        return serializer.Deserialize(reader);
    }
}