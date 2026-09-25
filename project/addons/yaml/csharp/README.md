# C# wrapper

`Yaml.cs` is a managed facade for the YAML GDExtension. It is intentionally a
normal C# class rather than a `Node`, so it can be used from gameplay code,
resources, and tests:

```csharp
using Godot;
using GodotYaml;

var result = Yaml.Parse("player:\n  health: 100");
if (result.HasError)
    GD.PushError(result.Error);
else
    GD.Print(result.Data); // Godot Dictionary/Variant data

var text = Yaml.TryStringify(new Godot.Collections.Dictionary
{
    ["name"] = "Knight",
    ["health"] = 100,
});
```

The wrapper returns `Godot.Variant` for YAML data because YAML values can be
any Godot Variant type. Use `Variant.AsGodotObject()` when a returned value is
a Godot object. `YamlResult` exposes parse location, multi-document, style, and
schema-validation information.

The bridge is loaded from `res://addons/yaml/csharp/YamlCSharpBridge.gd`, so
keep the `project` directory from this add-on in the project. No custom C# glue
build is required.
