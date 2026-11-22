using SlahManager.Infrastructure;
using SlahManager.Infrastructure.Processes;
using SlahManager.Models.Tweaks;

namespace SlahManager.Services.Platform;

public sealed class StartupService
{
    public const string TrayArgument = "--tray";

    private const string TaskName = "slahManager";

    public Task<bool> IsEnabledAsync() => Task.Run(() =>
    {
        try
        {
            var output = PowerShellRunner.RunOrThrow(
                $"if (Get-ScheduledTask -TaskName {PowerShellText.Quote(TaskName)} -ErrorAction SilentlyContinue) {{ 'yes' }}");
            return output == "yes";
        }
        catch (Exception exception)
        {
            Logger.Error("Startup task check failed", exception);
            return false;
        }
    });

    public Task<TweakResult> SetEnabledAsync(bool enabled) => Task.Run(() =>
    {
        try
        {
            PowerShellRunner.RunOrThrow(enabled ? RegisterScript() : UnregisterScript());
            Logger.Info(enabled ? "Startup task registered" : "Startup task removed");
            return TweakResult.Ok();
        }
        catch (Exception exception)
        {
            Logger.Error("Startup task change failed", exception);
            return TweakResult.Fail(exception.Message);
        }
    });

    private static string RegisterScript()
    {
        var path = Environment.ProcessPath ?? throw new InvalidOperationException("Executable path is unknown.");

        return
            $"$action = New-ScheduledTaskAction -Execute {PowerShellText.Quote(path)} -Argument {PowerShellText.Quote(TrayArgument)}; " +
            "$user = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name; " +
            "$trigger = New-ScheduledTaskTrigger -AtLogOn -User $user; " +
            "$principal = New-ScheduledTaskPrincipal -UserId $user -LogonType Interactive -RunLevel Highest; " +
            "$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries " +
            "-ExecutionTimeLimit ([TimeSpan]::Zero); " +
            $"Register-ScheduledTask -TaskName {PowerShellText.Quote(TaskName)} -Action $action -Trigger $trigger " +
            "-Principal $principal -Settings $settings -Force | Out-Null";
    }

    private static string UnregisterScript() =>
        $"Unregister-ScheduledTask -TaskName {PowerShellText.Quote(TaskName)} -Confirm:$false -ErrorAction SilentlyContinue";
}
