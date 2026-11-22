using SlahManager.Infrastructure;
using SlahManager.Models.Boost;
using SlahManager.Services.Platform;

namespace SlahManager.Services.Boost;

public sealed class PowerBooster
{
    private readonly BoostJournalStore _journal;

    public PowerBooster(BoostJournalStore journal)
    {
        _journal = journal;
    }

    public bool IsApplied { get; private set; }

    public void Apply()
    {
        if (IsApplied)
        {
            return;
        }

        var scheme = PowerProfile.ActiveScheme();
        var entries = new List<BoostJournalEntry>();

        foreach (var target in BoostPowerSettings.Targets)
        {
            if (PowerProfile.ReadAc(scheme, target.Subgroup, target.Setting) is { } original)
            {
                entries.Add(new BoostJournalEntry
                {
                    Subgroup = target.Subgroup,
                    Setting = target.Setting,
                    OriginalAcValue = original
                });
            }
        }

        _journal.Save(new BoostJournal { Scheme = scheme, Entries = entries });

        try
        {
            foreach (var target in BoostPowerSettings.Targets.Where(target => entries.Any(entry => entry.Setting == target.Setting)))
            {
                PowerProfile.WriteAc(scheme, target.Subgroup, target.Setting, target.AcValue);
            }

            PowerProfile.Activate(scheme);
        }
        catch
        {
            Restore();
            throw;
        }

        IsApplied = true;
    }

    public void Restore()
    {
        var journal = _journal.Load();
        IsApplied = false;

        if (journal is null)
        {
            return;
        }

        foreach (var entry in journal.Entries)
        {
            try
            {
                PowerProfile.WriteAc(journal.Scheme, entry.Subgroup, entry.Setting, entry.OriginalAcValue);
            }
            catch (Exception exception)
            {
                Logger.Error("Could not restore a power setting", exception);
            }
        }

        try
        {
            PowerProfile.Activate(PowerProfile.ActiveScheme());
        }
        catch (Exception exception)
        {
            Logger.Error("Could not reapply the power scheme", exception);
        }

        _journal.Delete();
    }
}
