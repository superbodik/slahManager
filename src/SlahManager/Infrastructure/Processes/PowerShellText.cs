namespace SlahManager.Infrastructure.Processes;

public static class PowerShellText
{
    public static string Quote(string value) => "'" + value.Replace("'", "''") + "'";

    public static string Array(IEnumerable<string> values) => "@(" + string.Join(",", values.Select(Quote)) + ")";
}
