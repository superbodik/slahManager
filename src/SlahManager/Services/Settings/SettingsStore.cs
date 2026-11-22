using System.Text.Json;
using SlahManager.Infrastructure;
using SlahManager.Models.Settings;

namespace SlahManager.Services.Settings;

public sealed class SettingsStore
{
    public AppSettings? Load()
    {
        if (!File.Exists(AppPaths.Settings))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(AppPaths.Settings), JsonSettings.Options);
        }
        catch (Exception exception)
        {
            Logger.Error("Could not read settings", exception);
            return null;
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.DataRoot);
            File.WriteAllText(AppPaths.Settings, JsonSerializer.Serialize(settings, JsonSettings.Options));
        }
        catch (Exception exception)
        {
            Logger.Error("Could not save settings", exception);
        }
    }
}
