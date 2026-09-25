using Godot;
using Godot.Collections;

namespace GodotYaml;

/// C# convenience methods for reading values from YAML dictionaries.
///
/// YAML values remain Godot Variants because YAML can contain arbitrary Godot
/// values. These helpers provide explicit, non-throwing access for common
/// scalar types and nested string-keyed mappings.
public static class YamlExtensions
{
    public static bool TryGetString(
        this Dictionary<string, Variant> dictionary,
        string key,
        out string value)
    {
        if (dictionary.TryGetValue(key, out var variant) &&
            variant.VariantType == Variant.Type.String)
        {
            value = variant.AsString();
            return true;
        }

        value = string.Empty;
        return false;
    }

    public static string GetString(
        this Dictionary<string, Variant> dictionary,
        string key,
        string defaultValue = "")
    {
        return dictionary.TryGetString(key, out var value) ? value : defaultValue;
    }

    public static string GetRequiredString(
        this Dictionary<string, Variant> dictionary,
        string key)
    {
        if (dictionary.TryGetString(key, out var value))
            return value;

        throw MissingOrInvalidValue(key, "string");
    }

    public static bool TryGetInt(
        this Dictionary<string, Variant> dictionary,
        string key,
        out long value)
    {
        if (dictionary.TryGetValue(key, out var variant) &&
            variant.VariantType == Variant.Type.Int)
        {
            value = variant.AsInt64();
            return true;
        }

        value = default;
        return false;
    }

    public static long GetInt(
        this Dictionary<string, Variant> dictionary,
        string key,
        long defaultValue = 0)
    {
        return dictionary.TryGetInt(key, out var value) ? value : defaultValue;
    }

    public static long GetRequiredInt(
        this Dictionary<string, Variant> dictionary,
        string key)
    {
        if (dictionary.TryGetInt(key, out var value))
            return value;

        throw MissingOrInvalidValue(key, "integer");
    }

    public static bool TryGetFloat(
        this Dictionary<string, Variant> dictionary,
        string key,
        out double value)
    {
        if (dictionary.TryGetValue(key, out var variant) &&
            variant.VariantType == Variant.Type.Float)
        {
            value = variant.AsDouble();
            return true;
        }

        value = default;
        return false;
    }

    public static double GetFloat(
        this Dictionary<string, Variant> dictionary,
        string key,
        double defaultValue = 0.0)
    {
        return dictionary.TryGetFloat(key, out var value) ? value : defaultValue;
    }

    public static double GetRequiredFloat(
        this Dictionary<string, Variant> dictionary,
        string key)
    {
        if (dictionary.TryGetFloat(key, out var value))
            return value;

        throw MissingOrInvalidValue(key, "floating-point number");
    }

    public static bool TryGetBool(
        this Dictionary<string, Variant> dictionary,
        string key,
        out bool value)
    {
        if (dictionary.TryGetValue(key, out var variant) &&
            variant.VariantType == Variant.Type.Bool)
        {
            value = variant.AsBool();
            return true;
        }

        value = default;
        return false;
    }

    public static bool GetBool(
        this Dictionary<string, Variant> dictionary,
        string key,
        bool defaultValue = false)
    {
        return dictionary.TryGetBool(key, out var value) ? value : defaultValue;
    }

    public static bool GetRequiredBool(
        this Dictionary<string, Variant> dictionary,
        string key)
    {
        if (dictionary.TryGetBool(key, out var value))
            return value;

        throw MissingOrInvalidValue(key, "boolean");
    }

    public static bool TryGetDictionary(
        this Dictionary<string, Variant> dictionary,
        string key,
        out Dictionary<string, Variant> value)
    {
        value = new Dictionary<string, Variant>();
        return dictionary.TryGetValue(key, out var variant) &&
               TryConvertDictionary(variant, value);
    }

    public static Dictionary<string, Variant> GetDictionary(
        this Dictionary<string, Variant> dictionary,
        string key)
    {
        if (dictionary.TryGetDictionary(key, out var value))
            return value;

        return new Dictionary<string, Variant>();
    }

    public static Dictionary<string, Variant> GetRequiredDictionary(
        this Dictionary<string, Variant> dictionary,
        string key)
    {
        if (dictionary.TryGetDictionary(key, out var value))
            return value;

        throw MissingOrInvalidValue(key, "mapping with string keys");
    }

    private static bool TryConvertDictionary(
        Variant variant,
        Dictionary<string, Variant> destination)
    {
        if (variant.VariantType != Variant.Type.Dictionary)
            return false;

        var source = variant.AsGodotDictionary();
        foreach (Variant key in source.Keys)
        {
            if (key.VariantType != Variant.Type.String)
            {
                destination.Clear();
                return false;
            }

            destination[key.AsString()] = source[key];
        }

        return true;
    }

    private static YamlException MissingOrInvalidValue(string key, string expectedType)
    {
        return new YamlException($"YAML value '{key}' is missing or is not a {expectedType}.");
    }
}
