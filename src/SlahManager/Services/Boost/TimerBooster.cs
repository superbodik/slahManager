using SlahManager.Infrastructure.Native;

namespace SlahManager.Services.Boost;

public sealed class TimerBooster
{
    private const double UnitsPerMillisecond = 10_000.0;

    private uint _requested;

    public bool IsHeld => _requested != 0;

    public double? CurrentMilliseconds =>
        Ntdll.NtQueryTimerResolution(out _, out _, out var current) == 0 ? current / UnitsPerMillisecond : null;

    public double? FinestMilliseconds =>
        Ntdll.NtQueryTimerResolution(out _, out var finest, out _) == 0 ? finest / UnitsPerMillisecond : null;

    public bool Acquire()
    {
        if (IsHeld)
        {
            return true;
        }

        if (Ntdll.NtQueryTimerResolution(out _, out var finest, out _) != 0)
        {
            return false;
        }

        if (Ntdll.NtSetTimerResolution(finest, true, out _) != 0)
        {
            return false;
        }

        _requested = finest;
        return true;
    }

    public void Release()
    {
        if (!IsHeld)
        {
            return;
        }

        Ntdll.NtSetTimerResolution(_requested, false, out _);
        _requested = 0;
    }
}
