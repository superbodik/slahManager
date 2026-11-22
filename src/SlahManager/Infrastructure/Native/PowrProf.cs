using System.Runtime.InteropServices;

namespace SlahManager.Infrastructure.Native;

internal static class PowrProf
{
    [DllImport("powrprof.dll")]
    public static extern uint PowerGetActiveScheme(IntPtr userRootKey, out IntPtr activePolicyGuid);

    [DllImport("powrprof.dll")]
    public static extern uint PowerSetActiveScheme(IntPtr userRootKey, ref Guid schemeGuid);

    [DllImport("powrprof.dll")]
    public static extern uint PowerReadACValueIndex(IntPtr rootKey, ref Guid schemeGuid, ref Guid subgroupGuid, ref Guid settingGuid, out uint acValueIndex);

    [DllImport("powrprof.dll")]
    public static extern uint PowerReadDCValueIndex(IntPtr rootKey, ref Guid schemeGuid, ref Guid subgroupGuid, ref Guid settingGuid, out uint dcValueIndex);

    [DllImport("powrprof.dll")]
    public static extern uint PowerWriteACValueIndex(IntPtr rootKey, ref Guid schemeGuid, ref Guid subgroupGuid, ref Guid settingGuid, uint acValueIndex);

    [DllImport("powrprof.dll")]
    public static extern uint PowerWriteDCValueIndex(IntPtr rootKey, ref Guid schemeGuid, ref Guid subgroupGuid, ref Guid settingGuid, uint dcValueIndex);
}
