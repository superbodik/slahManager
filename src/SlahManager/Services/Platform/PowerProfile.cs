using System.ComponentModel;
using System.Runtime.InteropServices;
using SlahManager.Infrastructure.Native;

namespace SlahManager.Services.Platform;

public static class PowerProfile
{
    private const uint Success = 0;

    public static Guid ActiveScheme()
    {
        Throw(PowrProf.PowerGetActiveScheme(IntPtr.Zero, out var buffer));
        try
        {
            return Marshal.PtrToStructure<Guid>(buffer);
        }
        finally
        {
            Kernel32.LocalFree(buffer);
        }
    }

    public static uint? ReadAc(Guid scheme, Guid subgroup, Guid setting) =>
        PowrProf.PowerReadACValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, out var value) == Success
            ? value
            : null;

    public static uint? ReadDc(Guid scheme, Guid subgroup, Guid setting) =>
        PowrProf.PowerReadDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, out var value) == Success
            ? value
            : null;

    public static void WriteAc(Guid scheme, Guid subgroup, Guid setting, uint value) =>
        Throw(PowrProf.PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value));

    public static void WriteDc(Guid scheme, Guid subgroup, Guid setting, uint value) =>
        Throw(PowrProf.PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value));

    public static void Activate(Guid scheme) =>
        Throw(PowrProf.PowerSetActiveScheme(IntPtr.Zero, ref scheme));

    private static void Throw(uint code)
    {
        if (code != Success)
        {
            throw new Win32Exception((int)code);
        }
    }
}
