using System.Text.Json;
using Microsoft.Win32;
using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Services.Tweaks.Executors;

internal static class RegistryValueConverter
{
    public static object ToObject(RegistryDataKind kind, JsonElement value) => kind switch
    {
        RegistryDataKind.Dword => unchecked((int)value.GetInt64()),
        RegistryDataKind.Qword => value.GetInt64(),
        _ => value.GetString() ?? string.Empty
    };

    public static RegistryValueKind ToValueKind(RegistryDataKind kind) => kind switch
    {
        RegistryDataKind.Dword => RegistryValueKind.DWord,
        RegistryDataKind.Qword => RegistryValueKind.QWord,
        _ => RegistryValueKind.String
    };

    public static bool Matches(object current, RegistryDataKind kind, JsonElement expected)
    {
        var target = ToObject(kind, expected);
        return kind switch
        {
            RegistryDataKind.Dword => current is int actual && actual == (int)target,
            RegistryDataKind.Qword => current is long actual && actual == (long)target,
            _ => current is string actual && string.Equals(actual, (string)target, StringComparison.Ordinal)
        };
    }
}
