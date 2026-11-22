namespace SlahManager.Models.Tweaks.Operations;

public sealed class ScheduledTaskOperation : OperationDefinition
{
    public string Path { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}
