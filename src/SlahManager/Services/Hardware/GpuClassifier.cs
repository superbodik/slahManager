using System.Text.RegularExpressions;
using SlahManager.Models.Hardware;

namespace SlahManager.Services.Hardware;

public static partial class GpuClassifier
{
    private static readonly string[] VirtualMarkers = ["microsoft", "virtual", "remote", "basic display", "parsec"];

    public static GpuVendor VendorOf(string name)
    {
        var upper = name.ToUpperInvariant();

        if (upper.Contains("NVIDIA") || upper.Contains("GEFORCE"))
        {
            return GpuVendor.Nvidia;
        }

        if (upper.Contains("AMD") || upper.Contains("RADEON"))
        {
            return GpuVendor.Amd;
        }

        return upper.Contains("INTEL") ? GpuVendor.Intel : GpuVendor.Unknown;
    }

    public static bool IsDiscrete(string name, GpuVendor vendor) => vendor switch
    {
        GpuVendor.Nvidia => true,
        GpuVendor.Amd => AmdDiscretePattern().IsMatch(name),
        GpuVendor.Intel => IntelDiscretePattern().IsMatch(name),
        _ => false
    };

    public static bool IsVirtual(string name) =>
        VirtualMarkers.Any(marker => name.Contains(marker, StringComparison.OrdinalIgnoreCase));

    [GeneratedRegex(@"\b(RX|PRO|FURY)\b", RegexOptions.IgnoreCase)]
    private static partial Regex AmdDiscretePattern();

    [GeneratedRegex(@"\bArc\b.*\b[AB]\d{3}", RegexOptions.IgnoreCase)]
    private static partial Regex IntelDiscretePattern();
}
