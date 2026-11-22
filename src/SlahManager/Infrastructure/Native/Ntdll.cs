using System.Runtime.InteropServices;

namespace SlahManager.Infrastructure.Native;

internal static class Ntdll
{
    [DllImport("ntdll.dll")]
    public static extern int NtQueryTimerResolution(out uint coarsest, out uint finest, out uint current);

    [DllImport("ntdll.dll")]
    public static extern int NtSetTimerResolution(uint desired, [MarshalAs(UnmanagedType.U1)] bool set, out uint current);
}
