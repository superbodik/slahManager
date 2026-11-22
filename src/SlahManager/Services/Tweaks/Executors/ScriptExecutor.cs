using SlahManager.Infrastructure.Processes;
using SlahManager.Models.Backup;
using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Services.Tweaks.Executors;

public sealed class ScriptExecutor : OperationExecutor<ScriptOperation>
{
    private const string StateKey = "state";

    protected override bool IsAppliedCore(ScriptOperation operation)
    {
        var output = Run(operation, operation.Check, null);
        var last = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).LastOrDefault();
        return string.Equals(last, "True", StringComparison.OrdinalIgnoreCase);
    }

    protected override OperationSnapshot CaptureCore(ScriptOperation operation)
    {
        var snapshot = new OperationSnapshot();
        snapshot.Data[StateKey] = Run(operation, operation.Capture, null).Trim();
        return snapshot;
    }

    protected override void ApplyCore(ScriptOperation operation) => Run(operation, operation.Apply, null);

    protected override void RestoreCore(ScriptOperation operation, OperationSnapshot? snapshot) =>
        Run(operation, operation.Restore, snapshot?.Data.GetValueOrDefault(StateKey));

    private static string Run(ScriptOperation operation, string body, string? state)
    {
        var stateLine = string.IsNullOrEmpty(state) ? "$state = $null; " : $"$state = {PowerShellText.Quote(state)}; ";
        return PowerShellRunner.RunOrThrow($"{stateLine}{operation.Prelude}; {body}");
    }
}
