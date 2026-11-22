namespace SlahManager.Models.Hardware;

public sealed record HardwareProfile
{
    public required string MachineName { get; init; }

    public required DeviceKind DeviceKind { get; init; }

    public required OsInfo Os { get; init; }

    public required CpuInfo Cpu { get; init; }

    public required MemoryInfo Memory { get; init; }

    public required StorageInfo Storage { get; init; }

    public required IReadOnlyList<GpuInfo> Gpus { get; init; }

    public bool HasDiscreteGpu => Gpus.Any(gpu => gpu.IsDiscrete);

    public GpuInfo? PrimaryGpu => Gpus.FirstOrDefault(gpu => gpu.IsDiscrete) ?? Gpus.FirstOrDefault();
}
