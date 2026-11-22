using System.Text.RegularExpressions;
using SlahManager.Infrastructure;
using SlahManager.Models.Hardware;

namespace SlahManager.Services.Hardware;

public static partial class CpuDetector
{
    public static CpuInfo Detect()
    {
        try
        {
            var rows = WmiClient.Query("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor");
            if (rows.Count > 0)
            {
                var name = Whitespace().Replace(WmiClient.Text(rows[0], "Name"), " ");
                var cores = (int)rows.Sum(row => WmiClient.Number(row, "NumberOfCores"));
                var threads = (int)rows.Sum(row => WmiClient.Number(row, "NumberOfLogicalProcessors"));
                return new CpuInfo(name, cores, threads);
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Processor detection failed", exception);
        }

        return new CpuInfo("Unknown processor", Environment.ProcessorCount, Environment.ProcessorCount);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
