using SlahManager.Infrastructure;
using SlahManager.Models.Hardware;

namespace SlahManager.Services.Hardware;

public static class DeviceKindDetector
{
    private static readonly HashSet<ushort> PortableChassisTypes = [8, 9, 10, 11, 12, 14, 18, 21, 30, 31, 32];

    public static DeviceKind Detect()
    {
        try
        {
            var enclosures = WmiClient.Query("SELECT ChassisTypes FROM Win32_SystemEnclosure");
            var portable = enclosures
                .Select(row => row["ChassisTypes"] as ushort[])
                .Any(types => types is not null && types.Any(PortableChassisTypes.Contains));
            return portable ? DeviceKind.Laptop : DeviceKind.Desktop;
        }
        catch (Exception exception)
        {
            Logger.Error("Chassis detection failed", exception);
            return DeviceKind.Desktop;
        }
    }
}
