using SlahManager.Infrastructure;
using SlahManager.Models.Hardware;

namespace SlahManager.Services.Hardware;

public static class GpuDetector
{
    public static IReadOnlyList<GpuInfo> Detect()
    {
        try
        {
            return WmiClient.Query("SELECT Name, DriverVersion FROM Win32_VideoController")
                .Select(row => new { Name = WmiClient.Text(row, "Name"), Driver = WmiClient.Text(row, "DriverVersion") })
                .Where(adapter => adapter.Name.Length > 0 && !GpuClassifier.IsVirtual(adapter.Name))
                .Select(adapter =>
                {
                    var vendor = GpuClassifier.VendorOf(adapter.Name);
                    return new GpuInfo(adapter.Name, vendor, GpuClassifier.IsDiscrete(adapter.Name, vendor), adapter.Driver);
                })
                .OrderByDescending(gpu => gpu.IsDiscrete)
                .ToList();
        }
        catch (Exception exception)
        {
            Logger.Error("Graphics detection failed", exception);
            return [];
        }
    }
}
