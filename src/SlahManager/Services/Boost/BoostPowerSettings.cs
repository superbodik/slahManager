using SlahManager.Models.Boost;

namespace SlahManager.Services.Boost;

public static class BoostPowerSettings
{
    private const string ProcessorSubgroup = "54533251-82be-4824-96c1-47b60b740d00";
    private const string PciExpressSubgroup = "501a4d13-42af-4429-9fd1-a8218c268e20";
    private const string UsbSubgroup = "2a737441-1930-4402-8d77-b2bebba308a3";

    public static IReadOnlyList<PowerSettingTarget> Targets { get; } =
    [
        new(Guid.Parse(ProcessorSubgroup), Guid.Parse("893dee8e-2bef-41e0-89c6-b55d0929964c"), 100),
        new(Guid.Parse(ProcessorSubgroup), Guid.Parse("0cc5b647-c1df-4637-891a-dec35c318583"), 100),
        new(Guid.Parse(PciExpressSubgroup), Guid.Parse("ee12f906-d277-404b-b6da-e5fa1a576df5"), 0),
        new(Guid.Parse(UsbSubgroup), Guid.Parse("48e6b7a6-50f5-4782-a5d4-53bb8f07e226"), 0)
    ];
}
