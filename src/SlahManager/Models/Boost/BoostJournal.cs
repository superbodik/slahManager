namespace SlahManager.Models.Boost;

public sealed class BoostJournal
{
    public Guid Scheme { get; init; }

    public List<BoostJournalEntry> Entries { get; init; } = [];
}
