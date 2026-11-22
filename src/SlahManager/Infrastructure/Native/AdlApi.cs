using System.Runtime.InteropServices;

namespace SlahManager.Infrastructure.Native;

internal static class AdlApi
{
    private const int Success = 0;
    private const int SaturationColor = 4;
    private const int DisplayInfoSize = 552;
    private const int LogicalIndexOffset = 0;
    private const int LogicalAdapterOffset = 8;
    private const int InfoValueOffset = 548;
    private const int ConnectedAndMapped = 0x3;

    private static readonly Malloc Allocate = size => Marshal.AllocCoTaskMem(size);
    private static readonly Lazy<Functions?> Loaded = new(Load);

    public static bool IsAvailable => Loaded.Value is not null;

    public static IReadOnlyList<(int Adapter, int Display)> ConnectedDisplays()
    {
        var functions = Loaded.Value;
        var displays = new HashSet<(int, int)>();

        if (functions is null)
        {
            return [];
        }

        functions.AdapterCount(functions.Context, out var adapters);

        for (var adapter = 0; adapter < adapters; adapter++)
        {
            if (functions.DisplayInfo(functions.Context, adapter, out var count, out var buffer, 0) != Success || buffer == IntPtr.Zero)
            {
                continue;
            }

            try
            {
                for (var index = 0; index < count; index++)
                {
                    var entry = buffer + index * DisplayInfoSize;
                    var value = Marshal.ReadInt32(entry, InfoValueOffset);
                    var logicalAdapter = Marshal.ReadInt32(entry, LogicalAdapterOffset);

                    if ((value & ConnectedAndMapped) == ConnectedAndMapped && logicalAdapter >= 0)
                    {
                        displays.Add((logicalAdapter, Marshal.ReadInt32(entry, LogicalIndexOffset)));
                    }
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }

        return displays.ToList();
    }

    public static (int Current, int Default, int Min, int Max)? ReadSaturation(int adapter, int display)
    {
        var functions = Loaded.Value;
        if (functions is null)
        {
            return null;
        }

        var status = functions.ColorGet(functions.Context, adapter, display, SaturationColor,
            out var current, out var defaultValue, out var min, out var max, out _);
        return status == Success ? (current, defaultValue, min, max) : null;
    }

    public static bool WriteSaturation(int adapter, int display, int value)
    {
        var functions = Loaded.Value;
        return functions is not null
            && functions.ColorSet(functions.Context, adapter, display, SaturationColor, value) == Success;
    }

    private static Functions? Load()
    {
        try
        {
            if (!NativeLibrary.TryLoad("atiadlxx.dll", out var library))
            {
                return null;
            }

            T Resolve<T>(string name) where T : Delegate =>
                Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));

            if (Resolve<Create>("ADL2_Main_Control_Create")(Allocate, 1, out var context) != Success)
            {
                return null;
            }

            return new Functions(
                context,
                Resolve<AdapterCountCall>("ADL2_Adapter_NumberOfAdapters_Get"),
                Resolve<DisplayInfoCall>("ADL2_Display_DisplayInfo_Get"),
                Resolve<ColorGetCall>("ADL2_Display_Color_Get"),
                Resolve<ColorSetCall>("ADL2_Display_Color_Set"));
        }
        catch (Exception exception)
        {
            Logger.Error("AMD display library could not be loaded", exception);
            return null;
        }
    }

    private sealed record Functions(
        IntPtr Context,
        AdapterCountCall AdapterCount,
        DisplayInfoCall DisplayInfo,
        ColorGetCall ColorGet,
        ColorSetCall ColorSet);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr Malloc(int size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int Create(Malloc malloc, int enumerateConnected, out IntPtr context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int AdapterCountCall(IntPtr context, out int count);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int DisplayInfoCall(IntPtr context, int adapter, out int count, out IntPtr info, int forceDetect);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int ColorGetCall(IntPtr context, int adapter, int display, int colorType, out int current, out int defaultValue, out int min, out int max, out int step);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int ColorSetCall(IntPtr context, int adapter, int display, int colorType, int value);
}
