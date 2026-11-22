using SlahManager.Models.Hardware;
using SlahManager.Models.Tweaks;

namespace SlahManager.Services.Tweaks;

public sealed class RequirementEvaluator
{
    public Availability Evaluate(TweakRequirements requirements, HardwareProfile hardware)
    {
        if (requirements.MinBuild is { } minBuild && hardware.Os.Build < minBuild)
        {
            return Availability.No("requirement.build", minBuild.ToString());
        }

        if (requirements.DiscreteGpu is true && !hardware.HasDiscreteGpu)
        {
            return Availability.No("requirement.discreteGpu");
        }

        if (requirements.GpuVendors.Count > 0 && !hardware.Gpus.Any(gpu => requirements.GpuVendors.Contains(gpu.Vendor)))
        {
            return Availability.No("requirement.gpuVendor", string.Join(" / ", requirements.GpuVendors.Select(DescribeVendor)));
        }

        if (requirements.DeviceKinds.Count == 1 && requirements.DeviceKinds[0] != hardware.DeviceKind)
        {
            return Availability.No($"requirement.device.{requirements.DeviceKinds[0].ToString().ToLowerInvariant()}");
        }

        if (requirements.StorageKinds.Count > 0 && !requirements.StorageKinds.Contains(hardware.Storage.Kind))
        {
            return Availability.No("requirement.storage");
        }

        return Availability.Yes;
    }

    private static string DescribeVendor(GpuVendor vendor) => vendor switch
    {
        GpuVendor.Nvidia => "NVIDIA",
        GpuVendor.Amd => "AMD",
        GpuVendor.Intel => "Intel",
        _ => vendor.ToString()
    };
}
