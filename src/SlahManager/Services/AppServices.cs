using SlahManager.Services.Boost;
using SlahManager.Services.Display;
using SlahManager.Services.Hardware;
using SlahManager.Services.Network;
using SlahManager.Services.Platform;
using SlahManager.Services.Settings;
using SlahManager.Services.Tweaks;
using SlahManager.Services.Tweaks.Executors;

namespace SlahManager.Services;

public sealed class AppServices
{
    public required IHardwareScanner Scanner { get; init; }

    public required TweakCatalog Catalog { get; init; }

    public required TweakEngine Engine { get; init; }

    public required RequirementEvaluator Requirements { get; init; }

    public required ProfileDetector Profiles { get; init; }

    public required ActivationService Activation { get; init; }

    public required RestorePointService RestorePoints { get; init; }

    public required RestorePointGuard RestorePointGuard { get; init; }

    public required SettingsService Settings { get; init; }

    public required GameBoostService Boost { get; init; }

    public required StartupService Startup { get; init; }

    public required PingTester Ping { get; init; }

    public required VibranceController Vibrance { get; init; }

    public required DialogService Dialogs { get; init; }

    public required ShellService Shell { get; init; }

    public static AppServices Create()
    {
        var executors = new OperationExecutorRegistry(
        [
            new RegistryExecutor(),
            new RegistryFlagExecutor(),
            new ServiceExecutor(),
            new ScheduledTaskExecutor(),
            new PowerPlanExecutor(),
            new PowerSettingExecutor(),
            new AppxExecutor(),
            new ScriptExecutor()
        ]);

        var settings = new SettingsService(new SettingsStore());
        var restorePoints = new RestorePointService();
        var vibrance = new VibranceController();

        var boost = new GameBoostService(
            () => settings.BoostOptions,
            new GameDetector(),
            new TimerBooster(),
            new ProcessBooster(),
            new PowerBooster(new BoostJournalStore()),
            new WifiScanBooster(new WifiJournalStore()),
            new VibranceBooster(vibrance, new VibranceJournalStore()));

        return new AppServices
        {
            Scanner = new HardwareScanner(),
            Catalog = new TweakCatalog(),
            Engine = new TweakEngine(executors, new BackupStore()),
            Requirements = new RequirementEvaluator(),
            Profiles = new ProfileDetector(),
            Activation = new ActivationService(),
            RestorePoints = restorePoints,
            RestorePointGuard = new RestorePointGuard(restorePoints, settings),
            Settings = settings,
            Boost = boost,
            Startup = new StartupService(),
            Ping = new PingTester(),
            Vibrance = vibrance,
            Dialogs = new DialogService(),
            Shell = new ShellService()
        };
    }
}
