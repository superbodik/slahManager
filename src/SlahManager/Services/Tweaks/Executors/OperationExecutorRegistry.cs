using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Services.Tweaks.Executors;

public sealed class OperationExecutorRegistry
{
    private readonly Dictionary<Type, IOperationExecutor> _executors;

    public OperationExecutorRegistry(IEnumerable<IOperationExecutor> executors)
    {
        _executors = executors.ToDictionary(executor => executor.OperationType);
    }

    public IOperationExecutor Resolve(OperationDefinition operation) =>
        _executors.TryGetValue(operation.GetType(), out var executor)
            ? executor
            : throw new NotSupportedException($"No executor registered for {operation.GetType().Name}.");
}
