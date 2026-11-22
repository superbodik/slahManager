namespace SlahManager.Models.Tweaks.Operations;

public sealed class RegistryFlagOperation : OperationDefinition
{
    public RegistryHiveKind Hive { get; init; }

    public string Path { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Flag { get; init; } = string.Empty;

    public string FlagValue { get; init; } = string.Empty;
}
