using System.Runtime.InteropServices;

namespace SlahManager.Infrastructure.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct ProcessPowerThrottlingState
{
    public const uint CurrentVersion = 1;
    public const uint ExecutionSpeed = 0x1;
    public const uint IgnoreTimerResolution = 0x4;

    public uint Version;
    public uint ControlMask;
    public uint StateMask;
}
