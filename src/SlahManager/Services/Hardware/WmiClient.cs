using System.Globalization;
using System.Management;

namespace SlahManager.Services.Hardware;

public static class WmiClient
{
    public static IReadOnlyList<ManagementBaseObject> Query(string query, string scope = @"root\cimv2")
    {
        using var searcher = new ManagementObjectSearcher(scope, query);
        return searcher.Get().Cast<ManagementBaseObject>().ToList();
    }

    public static string Text(ManagementBaseObject row, string property) =>
        row[property]?.ToString()?.Trim() ?? string.Empty;

    public static long Number(ManagementBaseObject row, string property) =>
        row[property] is null ? 0 : Convert.ToInt64(row[property], CultureInfo.InvariantCulture);
}
