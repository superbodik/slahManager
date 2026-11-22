using System.ComponentModel;
using System.Globalization;
using SlahManager.Models.Backup;
using SlahManager.Models.Tweaks.Operations;
using SlahManager.Services.Platform;

namespace SlahManager.Services.Tweaks.Executors;

public sealed class PowerSettingExecutor : OperationExecutor<PowerSettingOperation>
{
    private const string SchemeKey = "scheme";
    private const string AcKey = "ac";
    private const string DcKey = "dc";
    private const int NotFound = 2;

    protected override bool IsAppliedCore(PowerSettingOperation operation)
    {
        var scheme = PowerProfile.ActiveScheme();
        var subgroup = Guid.Parse(operation.SubgroupGuid);
        var setting = Guid.Parse(operation.SettingGuid);

        var acMatches = operation.AcValue is not { } ac || PowerProfile.ReadAc(scheme, subgroup, setting) == (uint)ac;
        var dcMatches = operation.DcValue is not { } dc || PowerProfile.ReadDc(scheme, subgroup, setting) == (uint)dc;
        return acMatches && dcMatches;
    }

    protected override OperationSnapshot CaptureCore(PowerSettingOperation operation)
    {
        var scheme = PowerProfile.ActiveScheme();
        var subgroup = Guid.Parse(operation.SubgroupGuid);
        var setting = Guid.Parse(operation.SettingGuid);

        var snapshot = new OperationSnapshot();
        snapshot.Data[SchemeKey] = scheme.ToString();
        snapshot.Data[AcKey] = PowerProfile.ReadAc(scheme, subgroup, setting)?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        snapshot.Data[DcKey] = PowerProfile.ReadDc(scheme, subgroup, setting)?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        return snapshot;
    }

    protected override void ApplyCore(PowerSettingOperation operation)
    {
        var scheme = PowerProfile.ActiveScheme();
        var subgroup = Guid.Parse(operation.SubgroupGuid);
        var setting = Guid.Parse(operation.SettingGuid);

        if (operation.AcValue is { } ac)
        {
            PowerProfile.WriteAc(scheme, subgroup, setting, (uint)ac);
        }

        if (operation.DcValue is { } dc)
        {
            PowerProfile.WriteDc(scheme, subgroup, setting, (uint)dc);
        }

        PowerProfile.Activate(scheme);
    }

    protected override void RestoreCore(PowerSettingOperation operation, OperationSnapshot? snapshot)
    {
        var subgroup = Guid.Parse(operation.SubgroupGuid);
        var setting = Guid.Parse(operation.SettingGuid);
        var active = PowerProfile.ActiveScheme();
        var scheme = snapshot is not null && Guid.TryParse(snapshot.Data.GetValueOrDefault(SchemeKey), out var saved) ? saved : active;

        var ac = snapshot is not null ? Parse(snapshot.Data.GetValueOrDefault(AcKey)) : (uint?)operation.DefaultAcValue;
        var dc = snapshot is not null ? Parse(snapshot.Data.GetValueOrDefault(DcKey)) : (uint?)operation.DefaultDcValue;

        try
        {
            if (operation.AcValue is not null && ac is { } acValue)
            {
                PowerProfile.WriteAc(scheme, subgroup, setting, acValue);
            }

            if (operation.DcValue is not null && dc is { } dcValue)
            {
                PowerProfile.WriteDc(scheme, subgroup, setting, dcValue);
            }
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == NotFound)
        {
            return;
        }

        PowerProfile.Activate(active);
    }

    private static uint? Parse(string? text) =>
        uint.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;
}
