using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using SlahManager.Infrastructure;

namespace SlahManager.Services.Platform;

public static class ElevationService
{
    public static bool IsElevated { get; } = Detect();

    public static void RestartElevated()
    {
        var path = Environment.ProcessPath;
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true, Verb = "runas" });
            Application.Current.Shutdown();
        }
        catch (Exception exception)
        {
            Logger.Error("Elevated restart was cancelled or failed", exception);
        }
    }

    private static bool Detect()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
