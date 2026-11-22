using System.Globalization;
using SlahManager.Infrastructure;
using SlahManager.Models.Hardware;

namespace SlahManager.Services.Hardware;

public static class StorageDetector
{
    private const long NvmeBusType = 17;
    private const long HddMediaType = 3;
    private const long SsdMediaType = 4;

    public static StorageInfo Detect()
    {
        try
        {
            var disks = WmiClient.Query(
                "SELECT DeviceId, FriendlyName, MediaType, BusType FROM MSFT_PhysicalDisk",
                @"root\Microsoft\Windows\Storage");

            var system = disks
                .OrderBy(disk => int.TryParse(WmiClient.Text(disk, "DeviceId"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : int.MaxValue)
                .FirstOrDefault();

            if (system is not null)
            {
                var busType = WmiClient.Number(system, "BusType");
                var mediaType = WmiClient.Number(system, "MediaType");
                var kind = busType == NvmeBusType ? StorageKind.Nvme
                    : mediaType == SsdMediaType ? StorageKind.Ssd
                    : mediaType == HddMediaType ? StorageKind.Hdd
                    : StorageKind.Unknown;
                return new StorageInfo(kind, WmiClient.Text(system, "FriendlyName"));
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Storage detection failed", exception);
        }

        return new StorageInfo(StorageKind.Unknown, "Unknown drive");
    }
}
