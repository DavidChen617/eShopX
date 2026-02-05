using System.Text.Json;
using System.Text.Json.Serialization;

namespace eShopX.Common.Extensions;

public static class JsonExtension
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new StreamJsonConverter() }
    };

    public static string ToJson(this object obj, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, options ?? SerializerOptions);
    }

    public static bool TryParseJson<T>(this string json, out T? obj, out string errMsg, JsonSerializerOptions? options = null) where T : class
    {
        errMsg = string.Empty;
        try
        {
            obj = JsonSerializer.Deserialize<T>(json, options);
            return true;
        }
        catch (Exception e)
        {
            obj = null;
            errMsg = e.Message;
            return false;
        }
    }
}

internal sealed class StreamJsonConverter : JsonConverter<Stream>
{
    public override Stream? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return null;
    }

    public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteString("type", value.GetType().Name);
        if (value.CanSeek)
        {
            writer.WriteNumber("length", value.Length);
        }
        writer.WriteEndObject();
    }
}