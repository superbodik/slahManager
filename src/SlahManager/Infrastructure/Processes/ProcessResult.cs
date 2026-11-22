namespace SlahManager.Infrastructure.Processes;

public sealed record ProcessResult(int ExitCode, string Output, string Error);
