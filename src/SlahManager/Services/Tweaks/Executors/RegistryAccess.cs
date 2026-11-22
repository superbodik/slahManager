using Microsoft.Win32;
using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Services.Tweaks.Executors;

internal static class RegistryAccess
{
    public static RegistryKey? Open(RegistryHiveKind hive, string path, bool writable)
    {
        using var root = OpenRoot(hive);
        return root.OpenSubKey(path, writable);
    }

    public static RegistryKey OpenOrCreate(RegistryHiveKind hive, string path)
    {
        using var root = OpenRoot(hive);
        return root.CreateSubKey(path, true);
    }

    private static RegistryKey OpenRoot(RegistryHiveKind hive) => RegistryKey.OpenBaseKey(
        hive == RegistryHiveKind.CurrentUser ? RegistryHive.CurrentUser : RegistryHive.LocalMachine,
        RegistryView.Registry64);
}
