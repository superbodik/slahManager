using System.Security.Cryptography;
using System.Text;

namespace SlahManager.Infrastructure;

public static class AppPaths
{
    public const string DataOverrideVariable = "SLAHMANAGER_DATA";

    public static string DataRoot { get; } = ResolveDataRoot();

    public static string Backups { get; } = Path.Combine(DataRoot, "backups");

    public static string Logs { get; } = Path.Combine(DataRoot, "logs");

    public static string Settings { get; } = Path.Combine(DataRoot, "settings.json");

    public static string BoostJournal { get; } = Path.Combine(DataRoot, "boost-journal.json");

    public static string WifiJournal { get; } = Path.Combine(DataRoot, "wifi-journal.json");

    public static string VibranceJournal { get; } = Path.Combine(DataRoot, "vibrance-journal.json");

    public static string InstanceKey { get; } =
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(DataRoot.ToLowerInvariant())))[..12];

    private static string ResolveDataRoot()
    {
        var overridePath = Environment.GetEnvironmentVariable(DataOverrideVariable);

        return string.IsNullOrWhiteSpace(overridePath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "slahManager")
            : Path.GetFullPath(overridePath);
    }
}
