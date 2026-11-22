namespace SlahManager.Models.Network;

public sealed record PingReport(ConnectionKind Connection, PingStats? Router, PingStats Internet);
