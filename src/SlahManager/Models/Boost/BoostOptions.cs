namespace SlahManager.Models.Boost;

public sealed record BoostOptions(
    bool Enabled,
    bool TimerResolution,
    bool ProcessPriority,
    bool CpuPower,
    bool PauseWifiScan,
    bool Cs2Vibrance,
    int VibranceLevel,
    IReadOnlyList<string> CustomGames)
{
    public string Signature =>
        $"{Enabled}|{TimerResolution}|{ProcessPriority}|{CpuPower}|{PauseWifiScan}|{Cs2Vibrance}|{VibranceLevel}|{string.Join(',', CustomGames)}";
}
