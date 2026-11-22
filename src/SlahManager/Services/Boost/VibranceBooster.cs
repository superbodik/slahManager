using SlahManager.Infrastructure;
using SlahManager.Models.Boost;
using SlahManager.Services.Display;

namespace SlahManager.Services.Boost;

public sealed class VibranceBooster
{
    private readonly VibranceController _controller;
    private readonly VibranceJournalStore _journal;

    public VibranceBooster(VibranceController controller, VibranceJournalStore journal)
    {
        _controller = controller;
        _journal = journal;
    }

    public int? AppliedPercent { get; private set; }

    public bool IsApplied => AppliedPercent is not null;

    public bool Apply(int percent)
    {
        if (AppliedPercent == percent)
        {
            return true;
        }

        if (IsApplied)
        {
            Restore();
        }

        var targets = _controller.Read();
        if (targets.Count == 0)
        {
            return false;
        }

        _journal.Save(targets
            .Select(target => new VibranceJournalEntry { Vendor = target.Vendor, Key = target.Key, Raw = target.Raw })
            .ToList());

        var changed = targets.Count(target => _controller.Write(target.Vendor, target.Key, target.RawFor(percent)));
        if (changed == 0)
        {
            Restore();
            return false;
        }

        AppliedPercent = percent;
        Logger.Info($"Vibrance set to {percent}% on {changed} display(s)");
        return true;
    }

    public void Restore()
    {
        var entries = _journal.Load();
        AppliedPercent = null;

        if (entries is null)
        {
            return;
        }

        foreach (var entry in entries)
        {
            if (!_controller.Write(entry.Vendor, entry.Key, entry.Raw))
            {
                Logger.Error($"Could not restore vibrance on {entry.Key}");
            }
        }

        _journal.Delete();
    }
}
