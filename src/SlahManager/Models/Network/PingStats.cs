namespace SlahManager.Models.Network;

public sealed record PingStats(string Target, double? AverageMilliseconds, double? JitterMilliseconds, double LossPercent)
{
    public bool Reachable => AverageMilliseconds is not null;
}
