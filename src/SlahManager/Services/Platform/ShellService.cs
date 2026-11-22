using System.Diagnostics;
using SlahManager.Infrastructure;

namespace SlahManager.Services.Platform;

public sealed class ShellService
{
    public void OpenFolder(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            Logger.Error($"Could not open {path}", exception);
        }
    }
}
