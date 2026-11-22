using SlahManager.Models.Backup;
using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Services.Tweaks.Executors;

public interface IOperationExecutor
{
    Type OperationType { get; }

    bool IsApplied(OperationDefinition operation);

    OperationSnapshot Capture(OperationDefinition operation);

    void Apply(OperationDefinition operation);

    void Restore(OperationDefinition operation, OperationSnapshot? snapshot);
}
