using Microsoft.Win32;
using SlahManager.Infrastructure.Processes;
using SlahManager.Models.Backup;
using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Services.Tweaks.Executors;

public sealed class ServiceExecutor : OperationExecutor<ServiceOperation>
{
    private const string StartKey = "start";

    protected override bool IsAppliedCore(ServiceOperation operation) =>
        ReadStart(operation.Name) is not { } start || start == (int)operation.Startup;

    protected override OperationSnapshot CaptureCore(ServiceOperation operation)
    {
        var snapshot = new OperationSnapshot();
        snapshot.Data[StartKey] = ReadStart(operation.Name)?.ToString() ?? string.Empty;
        return snapshot;
    }

    protected override void ApplyCore(ServiceOperation operation)
    {
        if (ReadStart(operation.Name) is null)
        {
            return;
        }

        Configure(operation.Name, (int)operation.Startup);

        if (operation.Startup == ServiceStartup.Disabled)
        {
            ProcessRunner.Run("sc.exe", ["stop", operation.Name]);
        }
    }

    protected override void RestoreCore(ServiceOperation operation, OperationSnapshot? snapshot)
    {
        if (ReadStart(operation.Name) is null)
        {
            return;
        }

        var start = snapshot is not null && int.TryParse(snapshot.Data.GetValueOrDefault(StartKey), out var saved)
            ? saved
            : (int)(operation.DefaultStartup ?? ServiceStartup.Manual);

        Configure(operation.Name, start);
    }

    private static int? ReadStart(string serviceName)
    {
        using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}");
        return key?.GetValue("Start") is int start ? start : null;
    }

    private static void Configure(string serviceName, int start)
    {
        var mode = start switch
        {
            2 => "auto",
            3 => "demand",
            4 => "disabled",
            _ => null
        };

        if (mode is null)
        {
            return;
        }

        var result = ProcessRunner.Run("sc.exe", ["config", serviceName, "start=", mode]);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"Could not configure service {serviceName}: {result.Output}");
        }
    }
}
