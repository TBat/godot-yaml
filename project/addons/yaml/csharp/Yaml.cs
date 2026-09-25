using Godot;
using Godot.Collections;
using System;

namespace GodotYaml;

/// Managed facade for the Godot YAML GDExtension.
///
/// The native extension is exposed to GDScript. This facade calls a tiny
/// GDScript bridge so it does not require generated C# glue for the custom
/// GDExtension classes. Add this file to the C# project; the bridge is loaded
/// from res://addons/yaml/csharp/YamlCSharpBridge.gd.
public static class Yaml
{
    private const string BridgePath = "res://addons/yaml/csharp/YamlCSharpBridge.gd";
    private static GodotObject? _bridge;

    private static GodotObject Bridge
    {
        get
        {
            if (_bridge is not null)
                return _bridge;

            var script = GD.Load<Script>(BridgePath)
                ?? throw new InvalidOperationException($"Could not load {BridgePath}. Is the YAML add-on installed?");
            _bridge = script.New().AsGodotObject()
                ?? throw new InvalidOperationException("Could not instantiate the YAML C# bridge.");
            return _bridge;
        }
    }

    private static Variant Call(string method, params Variant[] args) => Bridge.Call(method, args);

    public static string Version() => Call("version").AsString();

    public static YamlResult Parse(string input, bool detectStyle = false) =>
        Result(Call("parse", input, detectStyle));

    /// Parses YAML whose root value must be a mapping with string keys.
    public static Dictionary<string, Variant> ParseDictionary(string input, bool detectStyle = false) =>
        RequireDictionary(Parse(input, detectStyle));

    /// Parses YAML into a string-keyed dictionary without throwing for YAML errors.
    public static bool TryParseDictionary(
        string input,
        out Dictionary<string, Variant> dictionary,
        bool detectStyle = false)
    {
        var result = TryParse(input);
        return TryGetDictionary(result, out dictionary);
    }

    public static YamlResult ParseAndValidate(string input, Variant schema = default, bool detectStyle = false) =>
        Result(Call("parse_and_validate", input, schema, detectStyle));

    public static YamlResult Stringify(Variant value) => Result(Call("stringify", value));

    /// Serializes a string-keyed Godot dictionary.
    public static YamlResult Stringify(Dictionary<string, Variant> value) =>
        Stringify(new Variant(value));

    public static YamlResult LoadFile(string path, bool detectStyle = false) =>
        Result(Call("load_file", path, detectStyle));

    public static Dictionary<string, Variant> LoadDictionary(string path, bool detectStyle = false) =>
        RequireDictionary(LoadFile(path, detectStyle));

    public static YamlResult LoadFileAndValidate(string path, Variant schema = default, bool detectStyle = false) =>
        Result(Call("load_file_and_validate", path, schema, detectStyle));

    public static YamlResult SaveFile(Variant value, string path) => Result(Call("save_file", value, path));

    public static YamlResult SaveFile(Dictionary<string, Variant> value, string path) =>
        SaveFile(new Variant(value), path);

    public static YamlResult ValidateSyntax(string input) => Result(Call("validate_syntax", input));

    public static YamlResult ValidateFileSyntax(string path) => Result(Call("validate_file_syntax", path));

    public static Variant TryParse(string input) => Call("try_parse", input);

    public static Variant TryParseAndValidate(string input, Variant schema = default) =>
        Call("try_parse_and_validate", input, schema);

    public static string TryStringify(Variant value) => Call("try_stringify", value).AsString();

    public static string TryStringify(Dictionary<string, Variant> value) =>
        TryStringify(new Variant(value));

    public static Variant TryLoadFile(string path) => Call("try_load_file", path);

    public static Variant TryLoadFileAndValidate(string path, Variant schema = default) =>
        Call("try_load_file_and_validate", path, schema);

    public static bool TrySaveFile(Variant value, string path) => Call("try_save_file", value, path).AsBool();

    public static bool TrySaveFile(Dictionary<string, Variant> value, string path) =>
        TrySaveFile(new Variant(value), path);

    public static bool TryLoadDictionary(
        string path,
        out Dictionary<string, Variant> dictionary)
    {
        return TryGetDictionary(TryLoadFile(path), out dictionary);
    }

    public static GodotObject CreateStyle() => Object(Call("create_style"));

    public static GodotObject CreateSecurity() => Object(Call("create_security"));

    public static GodotObject LoadSchemaFromFile(string path, bool validateAgainstMeta = false) =>
        Object(Call("load_schema_from_file", path, validateAgainstMeta));

    public static GodotObject LoadSchemaFromString(string input, bool validateAgainstMeta = false) =>
        Object(Call("load_schema_from_string", input, validateAgainstMeta));

    private static YamlResult Result(Variant value) => new(Object(value));

    private static Dictionary<string, Variant> RequireDictionary(YamlResult result)
    {
        if (result.HasError)
            throw new YamlException(result.Error);

        if (!TryGetDictionary(result.Data, out var dictionary))
            throw new YamlException("The YAML root value is not a mapping with string keys.");

        return dictionary;
    }

    private static bool TryGetDictionary(
        Variant value,
        out Dictionary<string, Variant> dictionary)
    {
        dictionary = new Dictionary<string, Variant>();
        if (value.VariantType != Variant.Type.Dictionary)
            return false;

        var source = value.AsGodotDictionary();
        foreach (Variant key in source.Keys)
        {
            if (key.VariantType != Variant.Type.String)
            {
                dictionary.Clear();
                return false;
            }

            dictionary[key.AsString()] = source[key];
        }

        return true;
    }

    private static GodotObject Object(Variant value) =>
        value.AsGodotObject()
        ?? throw new InvalidOperationException("The YAML extension returned null.");
}

/// Exception thrown when a typed YAML helper cannot return the requested shape.
public sealed class YamlException : Exception
{
    public YamlException(string message) : base(message) { }
}

/// Managed view over the native YAMLResult object.
public sealed class YamlResult
{
    private readonly GodotObject _native;

    internal YamlResult(GodotObject native) => _native = native;

    public bool HasError => _native.Call("has_error").AsBool();
    public string Error => _native.Call("get_error").AsString();
    public string ErrorMessage => _native.Call("get_error_message").AsString();
    public long ErrorLine => _native.Call("get_error_line").AsInt64();
    public long ErrorColumn => _native.Call("get_error_column").AsInt64();
    public Variant Data => _native.Call("get_data");
    public int DocumentCount => (int)_native.Call("get_document_count").AsInt64();
    public bool HasMultipleDocuments => _native.Call("has_multiple_documents").AsBool();
    public bool HasStyle => _native.Call("has_style").AsBool();
    public bool HasValidationErrors => _native.Call("has_validation_errors").AsBool();
    public int ValidationErrorCount => (int)_native.Call("get_validation_error_count").AsInt64();
    public string ValidationSummary => _native.Call("get_validation_summary").AsString();

    public Variant GetDocument(int index = 0) => _native.Call("get_document", index);
    public Variant Documents => _native.Call("get_documents");
    public GodotObject? Style => _native.Call("get_style").AsGodotObject();
    public GodotObject? ValidationResult => _native.Call("get_validation_result").AsGodotObject();
    public Variant ValidationErrors => _native.Call("get_validation_errors");

    public override string ToString() => _native.ToString();
}
