using System.Text.Json;

namespace SlahManager.Models.Tweaks.Operations;

public sealed class RegistryOperation : OperationDefinition
{
    public RegistryHiveKind Hive { get; init; }

    public string Path { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public RegistryDataKind Kind { get; init; }

    public JsonElement Value { get; init; }

    public JsonElement? DefaultValue { get; init; }
}
