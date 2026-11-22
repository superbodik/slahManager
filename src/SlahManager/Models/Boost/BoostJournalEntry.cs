namespace SlahManager.Models.Boost;

public sealed class BoostJournalEntry
{
    public Guid Subgroup { get; init; }

    public Guid Setting { get; init; }

    public uint OriginalAcValue { get; init; }
}
