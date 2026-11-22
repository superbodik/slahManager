using SlahManager.Infrastructure.Processes;
using SlahManager.Models.Backup;
using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Services.Tweaks.Executors;

public sealed class ScheduledTaskExecutor : OperationExecutor<ScheduledTaskOperation>
{
    private const string StateKey = "state";
    private const string Missing = "Missing";
    private const string Disabled = "Disabled";

    protected override bool IsAppliedCore(ScheduledTaskOperation operation) =>
        ReadState(operation) is Missing or Disabled;

    protected override OperationSnapshot CaptureCore(ScheduledTaskOperation operation)
    {
        var snapshot = new OperationSnapshot();
        snapshot.Data[StateKey] = ReadState(operation);
        return snapshot;
    }

    protected override void ApplyCore(ScheduledTaskOperation operation)
    {
        if (ReadState(operation) == Missing)
        {
            return;
        }

        PowerShellRunner.RunOrThrow($"Disable-ScheduledTask {Target(operation)} | Out-Null");
    }

    protected override void RestoreCore(ScheduledTaskOperation operation, OperationSnapshot? snapshot)
    {
        var previous = snapshot?.Data.GetValueOrDefault(StateKey);
        if (previous is Missing or Disabled || ReadState(operation) == Missing)
        {
            return;
        }

        PowerShellRunner.RunOrThrow($"Enable-ScheduledTask {Target(operation)} | Out-Null");
    }

    private static string ReadState(ScheduledTaskOperation operation) => PowerShellRunner.RunOrThrow(
        $"$task = Get-ScheduledTask {Target(operation)} -ErrorAction SilentlyContinue; " +
        $"if ($null -eq $task) {{ '{Missing}' }} else {{ $task.State.ToString() }}");

    private static string Target(ScheduledTaskOperation operation) =>
        $"-TaskPath {PowerShellText.Quote(operation.Path)} -TaskName {PowerShellText.Quote(operation.Name)}";
}
