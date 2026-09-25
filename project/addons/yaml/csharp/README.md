# C# wrapper

This directory contains the managed C# facade for the Godot YAML GDExtension.
The API is split into two parts:

- `Yaml.cs` — parsing, serialization, validation, file I/O, and typed root-dictionary helpers.
- `YamlExtensions.cs` — convenient typed accessors for values inside `Dictionary<string, Variant>` objects.

The wrapper is intentionally implemented as normal C# classes rather than
`Node` subclasses, so it can be used from gameplay code, resources, and tests.

## Installation

Keep the add-on under `res://addons/yaml/`, including:

```text
res://addons/yaml/csharp/Yaml.cs
res://addons/yaml/csharp/YamlExtensions.cs
res://addons/yaml/csharp/YamlCSharpBridge.gd
```

The bridge is loaded from `res://addons/yaml/csharp/YamlCSharpBridge.gd`. No
custom C# glue build is required. Ensure the YAML GDExtension is installed and
enabled before calling the wrapper.

## Parse and inspect a result

Use `Parse` when you need detailed error information:

```csharp
using Godot;
using GodotYaml;

var result = Yaml.Parse("player:\n  health: 100");

if (result.HasError)
{
    GD.PushError($"YAML error at {result.ErrorLine}:{result.ErrorColumn}: {result.ErrorMessage}");
}
else
{
    GD.Print(result.Data); // Godot Variant containing the parsed data
}
```

`YamlResult` also exposes `DocumentCount`, `HasMultipleDocuments`, `GetDocument`,
`Documents`, style information, and schema-validation information.

## Parse a typed root dictionary

For YAML whose root value is a mapping with string keys, use
`ParseDictionary`. The dictionary values remain `Variant` because nested YAML
values can be any Godot Variant type:

```csharp
var config = Yaml.ParseDictionary("""
player:
  name: Knight
  health: 100
  speed: 2.5
  alive: true
""");

var player = config.GetRequiredDictionary("player");
GD.Print(player.GetRequiredString("name"));
GD.Print(player.GetRequiredInt("health"));
GD.Print(player.GetRequiredFloat("speed"));
GD.Print(player.GetRequiredBool("alive"));
```

`ParseDictionary` throws `YamlException` when parsing fails or when the root
value is not a string-keyed mapping. Use `TryParseDictionary` when you prefer a
boolean result instead:

```csharp
if (Yaml.TryParseDictionary("name: Hero\nlevel: 42", out var data))
{
    GD.Print(data.GetString("name"));
    GD.Print(data.GetInt("level"));
}
```

## Typed dictionary accessors

`YamlExtensions` provides three forms of access for common types:

- `TryGet...` returns `false` when the key is missing or has the wrong type.
- `Get...` returns a supplied default value, or the type default.
- `GetRequired...` throws `YamlException` when the key is missing or invalid.

Available accessors are:

```text
TryGetString / GetString / GetRequiredString
TryGetInt    / GetInt    / GetRequiredInt
TryGetFloat  / GetFloat  / GetRequiredFloat
TryGetBool   / GetBool   / GetRequiredBool
TryGetDictionary / GetDictionary / GetRequiredDictionary
```

Example:

```csharp
var settings = Yaml.ParseDictionary("""
name: Demo
max_players: 8
volume: 0.75
enabled: true
""");

var name = settings.GetString("name", "Unnamed");
var maxPlayers = settings.GetInt("max_players", 1);
var volume = settings.GetRequiredFloat("volume");
var enabled = settings.GetBool("enabled");

if (settings.TryGetInt("timeout", out var timeout))
    GD.Print($"Timeout: {timeout}");
```

Nested mappings can be read without manually converting `Variant` values:

```csharp
var document = Yaml.ParseDictionary("""
player:
  name: Hero
  stats:
    health: 100
    armor: 25
""");

var stats = document
    .GetRequiredDictionary("player")
    .GetRequiredDictionary("stats");

var health = stats.GetRequiredInt("health");
var armor = stats.GetRequiredInt("armor");
```

## Stringify and save dictionaries

The typed overloads accept `Godot.Collections.Dictionary<string, Variant>`
directly:

```csharp
using Godot.Collections;

var saveData = new Dictionary<string, Variant>
{
    ["player"] = new Dictionary<string, Variant>
    {
        ["name"] = "Knight",
        ["level"] = 10,
    },
    ["completed"] = true,
};

var yamlResult = Yaml.Stringify(saveData);
if (!yamlResult.HasError)
    GD.Print(yamlResult.Data.AsString());

Yaml.SaveFile(saveData, "user://save.yaml");
```

For simplified error handling, use `TryStringify` and `TrySaveFile`:

```csharp
var yamlText = Yaml.TryStringify(saveData);
var saved = Yaml.TrySaveFile(saveData, "user://save.yaml");
```

## Loading dictionaries from files

```csharp
var data = Yaml.LoadDictionary("user://settings.yaml");
var volume = data.GetFloat("volume", 1.0);
```

`LoadDictionary` throws `YamlException` for parse errors or a non-mapping root.
Use `TryLoadDictionary` when failure should be handled without exceptions:

```csharp
if (Yaml.TryLoadDictionary("user://settings.yaml", out var settings))
    GD.Print(settings.GetString("profile", "default"));
```

## Validation and schemas

The full result API remains available for schema validation:

```csharp
var schema = Yaml.LoadSchemaFromString("""
type: object
required: [name]
properties:
  name:
    type: string
""");

var result = Yaml.ParseAndValidate("name: Knight", schema);
if (result.HasError)
    GD.PushError(result.Error);
else if (result.HasValidationErrors)
    GD.PushError(result.ValidationSummary);
else
    GD.Print(result.Data);
```

Use `Yaml.TryParse`, `Yaml.TryParseAndValidate`, `Yaml.TryLoadFile`, and
`Yaml.TryLoadFileAndValidate` when you want the simplified Variant-returning
API. Use `Parse`, `LoadFile`, and their validation counterparts when you need
line/column details and a `YamlResult`.
