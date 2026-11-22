using Microsoft.Win32;
using SlahManager.Infrastructure;
using SlahManager.Models.Hardware;

namespace SlahManager.Services.Hardware;

public static class OsDetector
{
    private const string VersionKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    public static OsInfo Detect()
    {
        try
        {
            var rows = WmiClient.Query("SELECT Caption, BuildNumber FROM Win32_OperatingSystem");
            if (rows.Count > 0)
            {
                var caption = TrimVendorPrefix(WmiClient.Text(rows[0], "Caption"));
                var build = (int)WmiClient.Number(rows[0], "BuildNumber");
                var version = Registry.GetValue(VersionKey, "DisplayVersion", null) as string;
                return new OsInfo(string.IsNullOrEmpty(version) ? caption : $"{caption} {version}", build);
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Operating system detection failed", exception);
        }

        return new OsInfo("Windows", Environment.OSVersion.Version.Build);
    }

    private static string TrimVendorPrefix(string caption)
    {
        var index = caption.IndexOf("Windows", StringComparison.OrdinalIgnoreCase);
        return index > 0 ? caption[index..] : caption;
    }
}
