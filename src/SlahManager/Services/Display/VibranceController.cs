using System.Globalization;
using SlahManager.Infrastructure.Native;
using SlahManager.Models.Display;

namespace SlahManager.Services.Display;

public sealed class VibranceController
{
    public IReadOnlyList<VibranceTarget> Read()
    {
        var targets = new List<VibranceTarget>();

        foreach (var (name, handle) in NvApi.Displays())
        {
            if (NvApi.Read(handle) is { } info)
            {
                targets.Add(new VibranceTarget(DisplayVendor.Nvidia, name, info.Current, info.Min, info.Max, info.Default));
            }
        }

        foreach (var (adapter, display) in AdlApi.ConnectedDisplays())
        {
            if (AdlApi.ReadSaturation(adapter, display) is { } saturation)
            {
                targets.Add(new VibranceTarget(
                    DisplayVendor.Amd,
                    $"{adapter}:{display}",
                    saturation.Current,
                    saturation.Min,
                    saturation.Max,
                    saturation.Default));
            }
        }

        return targets;
    }

    public bool Write(DisplayVendor vendor, string key, int raw) => vendor switch
    {
        DisplayVendor.Nvidia => WriteNvidia(key, raw),
        DisplayVendor.Amd => WriteAmd(key, raw),
        _ => false
    };

    private static bool WriteNvidia(string displayName, int raw)
    {
        var display = NvApi.Displays().FirstOrDefault(entry => entry.Name == displayName);
        if (display.Handle == IntPtr.Zero)
        {
            return false;
        }

        return NvApi.Write(display.Handle, raw) && NvApi.Read(display.Handle)?.Current == raw;
    }

    private static bool WriteAmd(string key, int raw)
    {
        var parts = key.Split(':');
        return parts.Length == 2
            && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var adapter)
            && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var display)
            && AdlApi.WriteSaturation(adapter, display, raw);
    }
}
