namespace SlahManager.Models.Tweaks.Operations;

public sealed class PowerPlanOperation : OperationDefinition
{
    public string SourceGuid { get; init; } = string.Empty;

    public string TargetGuid { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}
