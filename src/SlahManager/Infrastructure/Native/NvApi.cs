using System.Runtime.InteropServices;
using System.Text;

namespace SlahManager.Infrastructure.Native;

internal static class NvApi
{
    private const uint InitializeId = 0x0150E828;
    private const uint EnumDisplayId = 0x9ABDD40D;
    private const uint DisplayNameId = 0x22A78B05;
    private const uint GetVibranceId = 0x0E45002D;
    private const uint SetVibranceId = 0x4A82C2B1;
    private const int MaxDisplays = 16;

    private static readonly Lazy<Functions?> Loaded = new(Load);

    public static bool IsAvailable => Loaded.Value is not null;

    public static IReadOnlyList<(string Name, IntPtr Handle)> Displays()
    {
        var functions = Loaded.Value;
        var displays = new List<(string, IntPtr)>();

        if (functions is null)
        {
            return displays;
        }

        for (var index = 0; index < MaxDisplays; index++)
        {
            if (functions.EnumDisplay(index, out var handle) != 0)
            {
                break;
            }

            var name = new StringBuilder(64);
            functions.DisplayName(handle, name);
            displays.Add((name.ToString(), handle));
        }

        return displays;
    }

    public static VibranceInfo? Read(IntPtr display)
    {
        var functions = Loaded.Value;
        if (functions is null)
        {
            return null;
        }

        var info = VibranceInfo.Create();
        return functions.GetVibrance(display, 0, ref info) == 0 ? info : null;
    }

    public static bool Write(IntPtr display, int level)
    {
        var functions = Loaded.Value;
        if (functions is null)
        {
            return false;
        }

        var info = VibranceInfo.Create();
        info.Current = level;
        return functions.SetVibrance(display, 0, ref info) == 0;
    }

    private static Functions? Load()
    {
        try
        {
            if (!NativeLibrary.TryLoad("nvapi64.dll", out var library))
            {
                return null;
            }

            var query = Marshal.GetDelegateForFunctionPointer<QueryInterface>(NativeLibrary.GetExport(library, "nvapi_QueryInterface"));

            T Resolve<T>(uint id) where T : Delegate
            {
                var pointer = query(id);
                return pointer == IntPtr.Zero
                    ? throw new EntryPointNotFoundException($"NVAPI function 0x{id:X8} is missing.")
                    : Marshal.GetDelegateForFunctionPointer<T>(pointer);
            }

            if (Resolve<Initialize>(InitializeId)() != 0)
            {
                return null;
            }

            return new Functions(
                Resolve<EnumDisplay>(EnumDisplayId),
                Resolve<DisplayName>(DisplayNameId),
                Resolve<VibranceCall>(GetVibranceId),
                Resolve<VibranceCall>(SetVibranceId));
        }
        catch (Exception exception)
        {
            Logger.Error("NVIDIA API could not be loaded", exception);
            return null;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct VibranceInfo
    {
        public uint Version;
        public int Current;
        public int Min;
        public int Max;
        public int Default;

        public static VibranceInfo Create() =>
            new() { Version = (uint)(Marshal.SizeOf<VibranceInfo>() | (1 << 16)) };
    }

    private sealed record Functions(EnumDisplay EnumDisplay, DisplayName DisplayName, VibranceCall GetVibrance, VibranceCall SetVibrance);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr QueryInterface(uint id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int Initialize();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int EnumDisplay(int index, out IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private delegate int DisplayName(IntPtr handle, StringBuilder name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int VibranceCall(IntPtr handle, uint outputId, ref VibranceInfo info);
}
