using SlahManager.Localization;
using SlahManager.Models.Hardware;
using SlahManager.ViewModels.Items;

namespace SlahManager.ViewModels.Builders;

public static class SpecGroupBuilder
{
    private static Localizer Text => Localizer.Instance;

    public static IReadOnlyList<SpecGroupViewModel> Build(HardwareProfile hardware, SpecRowViewModel activation)
    {
        var hardwareRows = new List<SpecRowViewModel>
        {
            new("spec.device", () => Text.Get(hardware.DeviceKind == DeviceKind.Laptop ? "device.laptop" : "device.desktop")),
            new("spec.processor", () => Text.Format("spec.processor.value", hardware.Cpu.Name, hardware.Cpu.Cores, hardware.Cpu.Threads))
        };

        hardwareRows.AddRange(hardware.Gpus.Select(gpu =>
            new SpecRowViewModel("spec.graphics", () =>
                Text.Format("spec.gpu.value", gpu.Name, Text.Get(gpu.IsDiscrete ? "gpu.discrete" : "gpu.integrated")))));

        hardwareRows.Add(new SpecRowViewModel("spec.memory", () => DescribeMemory(hardware.Memory)));
        hardwareRows.Add(new SpecRowViewModel("spec.storage", () =>
            Text.Format("spec.storage.value", Text.Get(StorageKey(hardware.Storage.Kind)), hardware.Storage.Name)));

        var windowsRows = new List<SpecRowViewModel>
        {
            new("spec.edition", () => hardware.Os.Name),
            new("spec.build", () => hardware.Os.Build.ToString()),
            activation
        };

        return
        [
            new SpecGroupViewModel("group.hardware", hardwareRows),
            new SpecGroupViewModel("group.windows", windowsRows)
        ];
    }

    private static string DescribeMemory(MemoryInfo memory) =>
        memory.SpeedMhz > 0 ? $"{memory.TotalGb} GB  ·  {memory.SpeedMhz} MHz" : $"{memory.TotalGb} GB";

    private static string StorageKey(StorageKind kind) => kind switch
    {
        StorageKind.Nvme => "storage.nvme",
        StorageKind.Ssd => "storage.ssd",
        StorageKind.Hdd => "storage.hdd",
        _ => "storage.unknown"
    };
}
