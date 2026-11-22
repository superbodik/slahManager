using System.Text.Json;
using SlahManager.Infrastructure;

namespace SlahManager.Services.Boost;

public sealed class WifiJournalStore
{
    public IReadOnlyList<string>? Load()
    {
        if (!File.Exists(AppPaths.WifiJournal))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(AppPaths.WifiJournal), JsonSettings.Options);
        }
        catch (Exception exception)
        {
            Logger.Error("Could not read the Wi-Fi journal", exception);
            return null;
        }
    }

    public void Save(IReadOnlyList<string> interfaces)
    {
        Directory.CreateDirectory(AppPaths.DataRoot);
        File.WriteAllText(AppPaths.WifiJournal, JsonSerializer.Serialize(interfaces, JsonSettings.Options));
    }

    public void Delete()
    {
        if (File.Exists(AppPaths.WifiJournal))
        {
            File.Delete(AppPaths.WifiJournal);
        }
    }
}
