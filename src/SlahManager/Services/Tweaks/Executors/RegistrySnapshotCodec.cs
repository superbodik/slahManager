using System.Globalization;
using Microsoft.Win32;
using SlahManager.Models.Backup;

namespace SlahManager.Services.Tweaks.Executors;

internal static class RegistrySnapshotCodec
{
    private const string ExistsKey = "exists";
    private const string KindKey = "kind";
    private const string ValueKey = "value";

    public static OperationSnapshot Encode(RegistryKey? key, string name)
    {
        var snapshot = new OperationSnapshot();
        var value = key?.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);

        if (key is null || value is null)
        {
            snapshot.Data[ExistsKey] = "0";
            return snapshot;
        }

        var kind = key.GetValueKind(name);
        snapshot.Data[ExistsKey] = "1";
        snapshot.Data[KindKey] = kind.ToString();
        snapshot.Data[ValueKey] = kind switch
        {
            RegistryValueKind.Binary => Convert.ToBase64String((byte[])value),
            RegistryValueKind.MultiString => string.Join('\0', (string[])value),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
        return snapshot;
    }

    public static bool Existed(OperationSnapshot snapshot) =>
        snapshot.Data.GetValueOrDefault(ExistsKey) == "1";

    public static void Decode(RegistryKey key, string name, OperationSnapshot snapshot)
    {
        if (!Existed(snapshot))
        {
            key.DeleteValue(name, false);
            return;
        }

        var kind = Enum.Parse<RegistryValueKind>(snapshot.Data[KindKey]);
        var raw = snapshot.Data[ValueKey];
        object value = kind switch
        {
            RegistryValueKind.DWord => int.Parse(raw, CultureInfo.InvariantCulture),
            RegistryValueKind.QWord => long.Parse(raw, CultureInfo.InvariantCulture),
            RegistryValueKind.Binary => Convert.FromBase64String(raw),
            RegistryValueKind.MultiString => raw.Split('\0'),
            _ => raw
        };
        key.SetValue(name, value, kind);
    }
}
