using SlahManager.Infrastructure;
using SlahManager.Models.Hardware;

namespace SlahManager.Services.Hardware;

public static class MemoryDetector
{
    private const long BytesPerGigabyte = 1024L * 1024 * 1024;

    public static MemoryInfo Detect()
    {
        try
        {
            var modules = WmiClient.Query("SELECT Capacity, Speed, ConfiguredClockSpeed FROM Win32_PhysicalMemory");
            if (modules.Count > 0)
            {
                var totalBytes = modules.Sum(module => WmiClient.Number(module, "Capacity"));
                var speed = modules.Max(module =>
                {
                    var configured = WmiClient.Number(module, "ConfiguredClockSpeed");
                    return configured > 0 ? configured : WmiClient.Number(module, "Speed");
                });
                return new MemoryInfo((int)Math.Round((double)totalBytes / BytesPerGigabyte), (int)speed);
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Memory detection failed", exception);
        }

        return new MemoryInfo(0, 0);
    }
}
