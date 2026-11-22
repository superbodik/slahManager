using System.Diagnostics;
using System.Text;

namespace SlahManager.Infrastructure.Processes;

public static class ProcessRunner
{
    public static ProcessResult Run(string fileName, IEnumerable<string> arguments, int timeoutMilliseconds = 180_000)
    {
        var info = new ProcessStartInfo(fileName)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        foreach (var argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }

        using var process = Process.Start(info)
            ?? throw new InvalidOperationException($"Could not start {fileName}.");

        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(timeoutMilliseconds))
        {
            try
            {
                process.Kill(true);
            }
            catch
            {
            }

            throw new TimeoutException($"{fileName} did not finish in time.");
        }

        process.WaitForExit();
        return new ProcessResult(process.ExitCode, output.Result.Trim(), error.Result.Trim());
    }
}
