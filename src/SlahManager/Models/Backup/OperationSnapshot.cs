namespace SlahManager.Models.Backup;

public sealed class OperationSnapshot
{
    public Dictionary<string, string> Data { get; init; } = new();
}
