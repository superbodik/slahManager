using Microsoft.Win32;
using SlahManager.Models.Backup;
using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Services.Tweaks.Executors;

public sealed class RegistryFlagExecutor : OperationExecutor<RegistryFlagOperation>
{
    protected override bool IsAppliedCore(RegistryFlagOperation operation)
    {
        using var key = RegistryAccess.Open(operation.Hive, operation.Path, false);
        return key?.GetValue(operation.Name) is string current
            && RegistryFlagString.Get(current, operation.Flag) == operation.FlagValue;
    }

    protected override OperationSnapshot CaptureCore(RegistryFlagOperation operation)
    {
        using var key = RegistryAccess.Open(operation.Hive, operation.Path, false);
        return RegistrySnapshotCodec.Encode(key, operation.Name);
    }

    protected override void ApplyCore(RegistryFlagOperation operation)
    {
        using var key = RegistryAccess.OpenOrCreate(operation.Hive, operation.Path);
        var current = key.GetValue(operation.Name) as string ?? string.Empty;
        key.SetValue(
            operation.Name,
            RegistryFlagString.With(current, operation.Flag, operation.FlagValue),
            RegistryValueKind.String);
    }

    protected override void RestoreCore(RegistryFlagOperation operation, OperationSnapshot? snapshot)
    {
        if (snapshot is not null && !RegistrySnapshotCodec.Existed(snapshot))
        {
            using var existing = RegistryAccess.Open(operation.Hive, operation.Path, true);
            existing?.DeleteValue(operation.Name, false);
            return;
        }

        using var key = RegistryAccess.OpenOrCreate(operation.Hive, operation.Path);

        if (snapshot is not null)
        {
            RegistrySnapshotCodec.Decode(key, operation.Name, snapshot);
            return;
        }

        var remaining = RegistryFlagString.Without(key.GetValue(operation.Name) as string ?? string.Empty, operation.Flag);
        if (remaining.Length == 0)
        {
            key.DeleteValue(operation.Name, false);
        }
        else
        {
            key.SetValue(operation.Name, remaining, RegistryValueKind.String);
        }
    }
}
