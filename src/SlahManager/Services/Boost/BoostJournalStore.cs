using System.Text.Json;
using SlahManager.Infrastructure;
using SlahManager.Models.Boost;

namespace SlahManager.Services.Boost;

public sealed class BoostJournalStore
{
    public BoostJournal? Load()
    {
        if (!File.Exists(AppPaths.BoostJournal))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<BoostJournal>(File.ReadAllText(AppPaths.BoostJournal), JsonSettings.Options);
        }
        catch (Exception exception)
        {
            Logger.Error("Could not read the boost journal", exception);
            return null;
        }
    }

    public void Save(BoostJournal journal)
    {
        Directory.CreateDirectory(AppPaths.DataRoot);
        File.WriteAllText(AppPaths.BoostJournal, JsonSerializer.Serialize(journal, JsonSettings.Options));
    }

    public void Delete()
    {
        if (File.Exists(AppPaths.BoostJournal))
        {
            File.Delete(AppPaths.BoostJournal);
        }
    }
}
