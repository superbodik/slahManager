using SlahManager.Localization;

namespace SlahManager.Models.Settings;

public sealed class AppSettings
{
    public AppLanguage Language { get; set; } = AppLanguage.English;

    public bool CreateRestorePoint { get; set; } = true;

    public bool RunInTray { get; set; } = true;

    public bool GameBoostEnabled { get; set; }

    public bool BoostTimerResolution { get; set; } = true;

    public bool BoostProcessPriority { get; set; } = true;

    public bool BoostCpuPower { get; set; } = true;

    public bool BoostPauseWifiScan { get; set; }

    public bool Cs2Vibrance { get; set; }

    public int Cs2VibranceLevel { get; set; } = 80;

    public List<string> CustomGames { get; set; } = [];
}
