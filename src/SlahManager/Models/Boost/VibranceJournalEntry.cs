using SlahManager.Models.Display;

namespace SlahManager.Models.Boost;

public sealed class VibranceJournalEntry
{
    public DisplayVendor Vendor { get; init; }

    public string Key { get; init; } = string.Empty;

    public int Raw { get; init; }
}
