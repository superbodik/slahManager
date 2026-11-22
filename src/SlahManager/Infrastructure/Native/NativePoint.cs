using System.Runtime.InteropServices;

namespace SlahManager.Infrastructure.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct NativePoint
{
    public int X;
    public int Y;
}
