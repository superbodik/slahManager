namespace SlahManager.Models.Backup;

public sealed class TweakBackup
{
    public string TweakId { get; init; } = string.Empty;

    public DateTime CreatedUtc { get; init; }

    public List<OperationSnapshot> Snapshots { get; init; } = [];
}
