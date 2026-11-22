namespace SlahManager.Models.Tweaks.Operations;

public sealed class PowerSettingOperation : OperationDefinition
{
    public string SubgroupGuid { get; init; } = string.Empty;

    public string SettingGuid { get; init; } = string.Empty;

    public long? AcValue { get; init; }

    public long? DcValue { get; init; }

    public long? DefaultAcValue { get; init; }

    public long? DefaultDcValue { get; init; }
}
