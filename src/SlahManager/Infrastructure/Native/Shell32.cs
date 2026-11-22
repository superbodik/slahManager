using System.Runtime.InteropServices;

namespace SlahManager.Infrastructure.Native;

internal static class Shell32
{
    public const uint NotifyAdd = 0x0;
    public const uint NotifyDelete = 0x2;
    public const uint FlagMessage = 0x1;
    public const uint FlagIcon = 0x2;
    public const uint FlagTip = 0x4;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Shell_NotifyIconW(uint message, ref NotifyIconData data);
}
