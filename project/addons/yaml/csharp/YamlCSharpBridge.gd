@tool
extends RefCounted

## Small GDScript bridge used by the C# facade.
##
## GDExtension classes are immediately available to GDScript, while a project
## using the standard Godot C# build does not generate managed types for custom
## GDExtension classes. Keeping this file in the add-on also means the facade
## works without a custom C# glue build.

func parse(input: String, detect_style: bool = false) -> Variant:
    return YAML.parse(input, null, detect_style)

func parse_and_validate(input: String, schema: Variant = null, detect_style: bool = false) -> Variant:
    return YAML.parse_and_validate(input, schema, null, detect_style)

func stringify(input: Variant) -> Variant:
    return YAML.stringify(input)

func load_file(path: String, detect_style: bool = false) -> Variant:
    return YAML.load_file(path, null, detect_style)

func load_file_and_validate(path: String, schema: Variant = null, detect_style: bool = false) -> Variant:
    return YAML.load_file_and_validate(path, schema, null, detect_style)

func save_file(data: Variant, path: String) -> Variant:
    return YAML.save_file(data, path)

func validate_syntax(input: String) -> Variant:
    return YAML.validate_syntax(input)

func validate_file_syntax(path: String) -> Variant:
    return YAML.validate_file_syntax(path)

func try_parse(input: String) -> Variant:
    return YAML.try_parse(input)

func try_parse_and_validate(input: String, schema: Variant = null) -> Variant:
    return YAML.try_parse_and_validate(input, schema)

func try_stringify(input: Variant) -> String:
    return YAML.try_stringify(input)

func try_load_file(path: String) -> Variant:
    return YAML.try_load_file(path)

func try_load_file_and_validate(path: String, schema: Variant = null) -> Variant:
    return YAML.try_load_file_and_validate(path, schema)

func try_save_file(data: Variant, path: String) -> bool:
    return YAML.try_save_file(data, path)

func create_style() -> Variant:
    return YAML.create_style()

func create_security() -> Variant:
    return YAML.create_security()

func load_schema_from_file(path: String, validate_against_meta: bool = false) -> Variant:
    return YAML.load_schema_from_file(path, validate_against_meta)

func load_schema_from_string(input: String, validate_against_meta: bool = false) -> Variant:
    return YAML.load_schema_from_string(input, validate_against_meta)

func version() -> String:
    return YAML.version()
