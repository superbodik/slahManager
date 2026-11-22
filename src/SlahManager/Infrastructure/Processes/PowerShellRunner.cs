using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SlahManager.Infrastructure.Processes;

public static partial class PowerShellRunner
{
    private const string Preamble =
        "$ProgressPreference='SilentlyContinue';$ErrorActionPreference='Stop';" +
        "[Console]::OutputEncoding=[System.Text.Encoding]::UTF8;";

    public static ProcessResult Run(string script)
    {
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(Preamble + script));
        return ProcessRunner.Run("powershell.exe",
        [
            "-NoLogo",
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy", "Bypass",
            "-EncodedCommand", encoded
        ]);
    }

    public static string RunOrThrow(string script)
    {
        var result = Run(script);
        if (result.ExitCode == 0)
        {
            return result.Output;
        }

        var message = CleanError(result.Error);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? result.Output : message);
    }

    private static string CleanError(string raw)
    {
        if (!raw.Contains("CLIXML", StringComparison.Ordinal))
        {
            return raw;
        }

        var parts = ErrorBlock().Matches(raw)
            .Select(match => match.Groups[1].Value.Replace("_x000D__x000A_", " ").Trim());
        var text = WebUtility.HtmlDecode(string.Join(" ", parts)).Trim();
        return text.Length > 0 ? text : raw;
    }

    [GeneratedRegex("<S S=\"Error\">(.*?)</S>", RegexOptions.Singleline)]
    private static partial Regex ErrorBlock();
}
