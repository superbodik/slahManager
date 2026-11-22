using System.Text.Json;
using SlahManager.Infrastructure;
using SlahManager.Models.Boost;

namespace SlahManager.Services.Boost;

public sealed class VibranceJournalStore
{
    public IReadOnlyList<VibranceJournalEntry>? Load()
    {
        if (!File.Exists(AppPaths.VibranceJournal))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<List<VibranceJournalEntry>>(File.ReadAllText(AppPaths.VibranceJournal), JsonSettings.Options);
        }
        catch (Exception exception)
        {
            Logger.Error("Could not read the vibrance journal", exception);
            return null;
        }
    }

    public void Save(IReadOnlyList<VibranceJournalEntry> entries)
    {
        Directory.CreateDirectory(AppPaths.DataRoot);
        File.WriteAllText(AppPaths.VibranceJournal, JsonSerializer.Serialize(entries, JsonSettings.Options));
    }

    public void Delete()
    {
        if (File.Exists(AppPaths.VibranceJournal))
        {
            File.Delete(AppPaths.VibranceJournal);
        }
    }
}
