using SlahManager.Models.Backup;
using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Services.Tweaks.Executors;

public sealed class RegistryExecutor : OperationExecutor<RegistryOperation>
{
    protected override bool IsAppliedCore(RegistryOperation operation)
    {
        using var key = RegistryAccess.Open(operation.Hive, operation.Path, false);
        var current = key?.GetValue(operation.Name);
        return current is not null && RegistryValueConverter.Matches(current, operation.Kind, operation.Value);
    }

    protected override OperationSnapshot CaptureCore(RegistryOperation operation)
    {
        using var key = RegistryAccess.Open(operation.Hive, operation.Path, false);
        return RegistrySnapshotCodec.Encode(key, operation.Name);
    }

    protected override void ApplyCore(RegistryOperation operation)
    {
        using var key = RegistryAccess.OpenOrCreate(operation.Hive, operation.Path);
        key.SetValue(
            operation.Name,
            RegistryValueConverter.ToObject(operation.Kind, operation.Value),
            RegistryValueConverter.ToValueKind(operation.Kind));
    }

    protected override void RestoreCore(RegistryOperation operation, OperationSnapshot? snapshot)
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
        }
        else if (operation.DefaultValue is { } fallback)
        {
            key.SetValue(
                operation.Name,
                RegistryValueConverter.ToObject(operation.Kind, fallback),
                RegistryValueConverter.ToValueKind(operation.Kind));
        }
        else
        {
            key.DeleteValue(operation.Name, false);
        }
    }
}
