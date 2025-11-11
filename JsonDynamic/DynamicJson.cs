using System.Collections;
using System.Dynamic;
using System.Text.Json;

namespace JsonDynamic;

public sealed class DynamicJson : DynamicObject, IEnumerable
{
    private readonly JsonElement _element;

    private DynamicJson(JsonElement element)
    {
        _element = element;
    }

    public static dynamic Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return new DynamicJson(JsonDocument.Parse(json).RootElement.Clone());
    }
    
    public static async Task<dynamic> ParseAsync(Stream stream)
    {
        using var doc = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
        return new DynamicJson(doc.RootElement.Clone());
    }
    
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (_element.ValueKind == JsonValueKind.Object &&
            _element.TryGetProperty(binder.Name, out var property))
        {
            result = Wrap(property);
            return true;
        }

        result = null;
        return true;
    }

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
    {
        if (indexes.Length == 1)
        {
            if (indexes[0] is int i && _element.ValueKind == JsonValueKind.Array)
            {
                var array = _element.EnumerateArray();
                int idx = 0;
                foreach (var item in array)
                {
                    if (idx++ == i)
                    {
                        result = Wrap(item);
                        return true;
                    }
                }
            }
            else if (indexes[0] is string key &&
                     _element.ValueKind == JsonValueKind.Object &&
                     _element.TryGetProperty(key, out var property))
            {
                result = Wrap(property);
                return true;
            }
        }

        result = null;
        return true;
    }

    public override bool TryConvert(ConvertBinder binder, out object? result)
    {
        try
        {
            switch (_element.ValueKind)
            {
                case JsonValueKind.String:
                    result = _element.GetString();
                    return true;
                case JsonValueKind.Number:
                    if (binder.Type == typeof(int))
                    {
                        result = _element.GetInt32();
                        return true;
                    }
                    if (binder.Type == typeof(long))
                    {
                        result = _element.GetInt64();
                        return true;
                    }
                    if (binder.Type == typeof(double))
                    {
                        result = _element.GetDouble();
                        return true;
                    }
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    result = _element.GetBoolean();
                    return true;
            }
        }
        catch
        {
            // Ignore et retourne null
        }

        result = null;
        return false;
    }

    public override string? ToString()
    {
        return _element.ValueKind switch
        {
            JsonValueKind.String => _element.GetString(),
            JsonValueKind.Number => _element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            _ => _element.ToString()
        };
    }

    public IEnumerator GetEnumerator()
    {
        if (_element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in _element.EnumerateArray())
                yield return Wrap(item);
        }
        else if (_element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in _element.EnumerateObject())
                yield return new KeyValuePair<string, object?>(prop.Name, Wrap(prop.Value));
        }
    }

    private static object? Wrap(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object or JsonValueKind.Array => new DynamicJson(element),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => TryGetNumber(element),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    private static object TryGetNumber(JsonElement e)
    {
        if (e.TryGetInt64(out var l)) return l;
        if (e.TryGetDouble(out var d)) return d;
        return e.GetRawText();
    }
}