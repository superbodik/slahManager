using SlahManager.Models.Hardware;
using SlahManager.Models.Profiles;

namespace SlahManager.Services.Platform;

public sealed class ProfileDetector
{
    public const string Gaming = "gaming";
    public const string Everyday = "everyday";

    public SystemProfile Detect(HardwareProfile hardware)
    {
        var form = hardware.DeviceKind == DeviceKind.Laptop ? "laptop" : "desktop";

        if (hardware.Gpus.FirstOrDefault(gpu => gpu.IsDiscrete) is { } discrete)
        {
            return new SystemProfile(Gaming, $"profile.gaming.{form}", "profile.gaming.summary", discrete.Name);
        }

        return new SystemProfile(Everyday, $"profile.everyday.{form}", "profile.everyday.summary");
    }
}
