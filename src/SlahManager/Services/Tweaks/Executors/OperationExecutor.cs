using SlahManager.Models.Backup;
using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Services.Tweaks.Executors;

public abstract class OperationExecutor<TOperation> : IOperationExecutor
    where TOperation : OperationDefinition
{
    public Type OperationType => typeof(TOperation);

    public bool IsApplied(OperationDefinition operation) => IsAppliedCore((TOperation)operation);

    public OperationSnapshot Capture(OperationDefinition operation) => CaptureCore((TOperation)operation);

    public void Apply(OperationDefinition operation) => ApplyCore((TOperation)operation);

    public void Restore(OperationDefinition operation, OperationSnapshot? snapshot) =>
        RestoreCore((TOperation)operation, snapshot);

    protected abstract bool IsAppliedCore(TOperation operation);

    protected abstract OperationSnapshot CaptureCore(TOperation operation);

    protected abstract void ApplyCore(TOperation operation);

    protected abstract void RestoreCore(TOperation operation, OperationSnapshot? snapshot);
}
