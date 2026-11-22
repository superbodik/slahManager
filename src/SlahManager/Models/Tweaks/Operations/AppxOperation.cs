namespace SlahManager.Models.Tweaks.Operations;

public sealed class AppxOperation : OperationDefinition
{
    public List<string> Packages { get; init; } = [];
}
