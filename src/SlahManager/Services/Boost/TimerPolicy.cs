using Microsoft.Win32;

namespace SlahManager.Services.Boost;

public static class TimerPolicy
{
    private const int FirstBuildWithPerProcessTimers = 22000;
    private const string KernelKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel";
    private const string ValueName = "GlobalTimerResolutionRequests";

    public static bool IsSystemWide()
    {
        if (Environment.OSVersion.Version.Build < FirstBuildWithPerProcessTimers)
        {
            return true;
        }

        using var key = Registry.LocalMachine.OpenSubKey(KernelKey);
        return key?.GetValue(ValueName) is int value && value == 1;
    }
}
