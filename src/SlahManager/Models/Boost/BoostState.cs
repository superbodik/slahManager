namespace SlahManager.Models.Boost;

public sealed record BoostState(
    bool IsActive,
    IReadOnlyList<GameProcess> Games,
    double? TimerMilliseconds,
    bool PriorityApplied,
    bool PowerApplied,
    bool PowerBlocked,
    bool WifiPaused,
    bool WifiBlocked,
    int? VibrancePercent,
    bool VibranceBlocked)
{
    public static BoostState Idle { get; } = new(false, [], null, false, false, false, false, false, null, false);

    public string Signature =>
        $"{IsActive}|{string.Join(',', Games.Select(game => game.ProcessId))}|{TimerMilliseconds:F2}|{PriorityApplied}|{PowerApplied}|{PowerBlocked}|{WifiPaused}|{WifiBlocked}|{VibrancePercent}|{VibranceBlocked}";
}
