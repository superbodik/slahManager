using System.Text.RegularExpressions;
using SlahManager.Infrastructure.Processes;
using SlahManager.Models.Backup;
using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Services.Tweaks.Executors;

public sealed partial class PowerPlanExecutor : OperationExecutor<PowerPlanOperation>
{
    private const string SchemeKey = "scheme";
    private const string BalancedScheme = "381b4222-f694-41f0-9685-ff5bb260df2e";

    protected override bool IsAppliedCore(PowerPlanOperation operation) =>
        string.Equals(ActiveScheme(), operation.TargetGuid, StringComparison.OrdinalIgnoreCase);

    protected override OperationSnapshot CaptureCore(PowerPlanOperation operation)
    {
        var snapshot = new OperationSnapshot();
        snapshot.Data[SchemeKey] = ActiveScheme() ?? BalancedScheme;
        return snapshot;
    }

    protected override void ApplyCore(PowerPlanOperation operation)
    {
        if (!SchemeExists(operation.TargetGuid))
        {
            Run("/duplicatescheme", operation.SourceGuid, operation.TargetGuid);
            Run("/changename", operation.TargetGuid, operation.Name);
        }

        Run("/setactive", operation.TargetGuid);
    }

    protected override void RestoreCore(PowerPlanOperation operation, OperationSnapshot? snapshot)
    {
        var previous = snapshot?.Data.GetValueOrDefault(SchemeKey);
        var target = !string.IsNullOrEmpty(previous) && SchemeExists(previous) ? previous : BalancedScheme;

        Run("/setactive", target);

        if (SchemeExists(operation.TargetGuid))
        {
            Run("/delete", operation.TargetGuid);
        }
    }

    private static string? ActiveScheme()
    {
        var match = GuidPattern().Match(ProcessRunner.Run("powercfg.exe", ["/getactivescheme"]).Output);
        return match.Success ? match.Value : null;
    }

    private static bool SchemeExists(string guid) =>
        ProcessRunner.Run("powercfg.exe", ["/list"]).Output.Contains(guid, StringComparison.OrdinalIgnoreCase);

    private static void Run(params string[] arguments)
    {
        var result = ProcessRunner.Run("powercfg.exe", arguments);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"powercfg {arguments[0]} failed: {result.Output} {result.Error}".Trim());
        }
    }

    [GeneratedRegex("[0-9a-fA-F]{8}(-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12}")]
    private static partial Regex GuidPattern();
}
