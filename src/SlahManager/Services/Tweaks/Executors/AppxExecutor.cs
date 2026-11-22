using SlahManager.Infrastructure.Processes;
using SlahManager.Models.Backup;
using SlahManager.Models.Tweaks.Operations;
using SlahManager.Services.Platform;

namespace SlahManager.Services.Tweaks.Executors;

public sealed class AppxExecutor : OperationExecutor<AppxOperation>
{
    protected override bool IsAppliedCore(AppxOperation operation)
    {
        var scope = ElevationService.IsElevated ? "-AllUsers" : string.Empty;
        var output = PowerShellRunner.RunOrThrow(
            $"$names = {PowerShellText.Array(operation.Packages)}; " +
            $"@(Get-AppxPackage {scope} | Where-Object {{ $names -contains $_.Name }}).Count");
        return int.TryParse(output, out var installed) && installed == 0;
    }

    protected override OperationSnapshot CaptureCore(AppxOperation operation) => new();

    protected override void ApplyCore(AppxOperation operation)
    {
        PowerShellRunner.RunOrThrow(
            $"$names = {PowerShellText.Array(operation.Packages)}; " +
            "foreach ($name in $names) { " +
            "Get-AppxPackage -AllUsers -Name $name | ForEach-Object { " +
            "Remove-AppxPackage -Package $_.PackageFullName -AllUsers -ErrorAction SilentlyContinue }; " +
            "Get-AppxProvisionedPackage -Online | Where-Object { $_.DisplayName -eq $name } | ForEach-Object { " +
            "Remove-AppxProvisionedPackage -Online -PackageName $_.PackageName -ErrorAction SilentlyContinue | Out-Null } }");
    }

    protected override void RestoreCore(AppxOperation operation, OperationSnapshot? snapshot)
    {
        throw new NotSupportedException("Removed apps cannot be restored from here.");
    }
}
