using SlahManager.Models.Hardware;

namespace SlahManager.Models.Tweaks;

public sealed class TweakRequirements
{
    public int? MinBuild { get; init; }

    public bool? DiscreteGpu { get; init; }

    public List<GpuVendor> GpuVendors { get; init; } = [];

    public List<DeviceKind> DeviceKinds { get; init; } = [];

    public List<StorageKind> StorageKinds { get; init; } = [];
}
