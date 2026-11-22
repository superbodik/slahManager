using SlahManager.Infrastructure;
using SlahManager.Infrastructure.Processes;
using SlahManager.Models.Tweaks;

namespace SlahManager.Services.Platform;

public sealed class RestorePointService
{
    private const string Script =
        "$drive = $env:SystemDrive + '\\'; " +
        "Enable-ComputerRestore -Drive $drive; " +
        "Checkpoint-Computer -Description 'slahManager' -RestorePointType 'MODIFY_SETTINGS'";

    public Task<TweakResult> CreateAsync() => Task.Run(() =>
    {
        try
        {
            PowerShellRunner.RunOrThrow(Script);
            Logger.Info("Restore point created");
            return TweakResult.Ok();
        }
        catch (Exception exception)
        {
            Logger.Error("Restore point failed", exception);
            return TweakResult.Fail(exception.Message);
        }
    });
}
