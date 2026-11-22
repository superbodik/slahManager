using SlahManager.Models.Hardware;

namespace SlahManager.Services.Hardware;

public sealed class HardwareScanner : IHardwareScanner
{
    public HardwareProfile Scan() => new()
    {
        MachineName = Environment.MachineName,
        DeviceKind = DeviceKindDetector.Detect(),
        Os = OsDetector.Detect(),
        Cpu = CpuDetector.Detect(),
        Memory = MemoryDetector.Detect(),
        Storage = StorageDetector.Detect(),
        Gpus = GpuDetector.Detect()
    };
}
