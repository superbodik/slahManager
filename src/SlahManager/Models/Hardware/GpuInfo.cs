namespace SlahManager.Models.Hardware;

public sealed record GpuInfo(string Name, GpuVendor Vendor, bool IsDiscrete, string DriverVersion);
