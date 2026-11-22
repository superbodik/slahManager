namespace SlahManager.Models.Tweaks.Operations;

public sealed class ScriptOperation : OperationDefinition
{
    public string Prelude { get; init; } = string.Empty;

    public string Check { get; init; } = string.Empty;

    public string Capture { get; init; } = string.Empty;

    public string Apply { get; init; } = string.Empty;

    public string Restore { get; init; } = string.Empty;
}
