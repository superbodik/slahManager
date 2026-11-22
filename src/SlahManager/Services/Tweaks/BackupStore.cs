using System.Text.Json;
using SlahManager.Infrastructure;
using SlahManager.Models.Backup;

namespace SlahManager.Services.Tweaks;

public sealed class BackupStore
{
    public bool Exists(string tweakId) => File.Exists(PathFor(tweakId));

    public TweakBackup? Load(string tweakId)
    {
        var path = PathFor(tweakId);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TweakBackup>(File.ReadAllText(path), JsonSettings.Options);
        }
        catch (Exception exception)
        {
            Logger.Error($"Could not read backup for {tweakId}", exception);
            return null;
        }
    }

    public void Save(TweakBackup backup)
    {
        Directory.CreateDirectory(AppPaths.Backups);
        File.WriteAllText(PathFor(backup.TweakId), JsonSerializer.Serialize(backup, JsonSettings.Options));
    }

    public void Delete(string tweakId)
    {
        var path = PathFor(tweakId);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static string PathFor(string tweakId) => Path.Combine(AppPaths.Backups, tweakId + ".json");
}
