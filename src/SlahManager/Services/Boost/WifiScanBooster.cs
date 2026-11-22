using System.Net.NetworkInformation;
using SlahManager.Infrastructure;
using SlahManager.Infrastructure.Processes;

namespace SlahManager.Services.Boost;

public sealed class WifiScanBooster
{
    private readonly WifiJournalStore _journal;

    public WifiScanBooster(WifiJournalStore journal)
    {
        _journal = journal;
    }

    public bool IsApplied { get; private set; }

    public bool Apply()
    {
        if (IsApplied)
        {
            return true;
        }

        var connected = NetworkInterface.GetAllNetworkInterfaces()
            .Where(adapter => adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                && adapter.OperationalStatus == OperationalStatus.Up)
            .Select(adapter => adapter.Name)
            .ToList();

        if (connected.Count == 0)
        {
            return true;
        }

        _journal.Save(connected);
        var paused = connected.Where(name => SetAutoConfig(name, false)).ToList();

        if (paused.Count == 0)
        {
            _journal.Delete();
            return false;
        }

        _journal.Save(paused);
        IsApplied = true;
        return true;
    }

    public void Restore()
    {
        var interfaces = _journal.Load();
        IsApplied = false;

        if (interfaces is null)
        {
            return;
        }

        foreach (var name in interfaces)
        {
            SetAutoConfig(name, true);
        }

        _journal.Delete();
    }

    private static bool SetAutoConfig(string interfaceName, bool enabled)
    {
        try
        {
            var result = ProcessRunner.Run(
                "netsh.exe",
                ["wlan", "set", "autoconfig", enabled ? "enabled=yes" : "enabled=no", $"interface={interfaceName}"]);

            if (result.ExitCode == 0)
            {
                return true;
            }

            Logger.Error($"netsh could not change Wi-Fi scanning on {interfaceName}: {result.Output}");
        }
        catch (Exception exception)
        {
            Logger.Error($"Wi-Fi scanning change failed on {interfaceName}", exception);
        }

        return false;
    }
}
