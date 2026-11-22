namespace SlahManager.Services.Boost;

public static class GameLocations
{
    private static readonly string[] LibraryMarkers =
    [
        @"\steamapps\common\",
        @"\epic games\",
        @"\gog galaxy\games\",
        @"\gog games\",
        @"\riot games\",
        @"\ubisoft game launcher\games\",
        @"\ea games\",
        @"\origin games\",
        @"\xboxgames\",
        @"\rockstar games\"
    ];

    private static readonly string[] ExcludedNameParts =
    [
        "launcher", "client", "helper", "crash", "report", "unins", "setup", "redist", "install",
        "update", "easyanticheat", "battleye", "overlay", "service", "bootstrap", "prereq", "webview", "cef"
    ];

    public static bool IsLibraryPath(string path)
    {
        var normalized = path.Replace('/', '\\').ToLowerInvariant();
        return LibraryMarkers.Any(normalized.Contains);
    }

    public static bool IsExcludedName(string processName) =>
        ExcludedNameParts.Any(part => processName.Contains(part, StringComparison.OrdinalIgnoreCase));

    public static string NormalizeName(string name)
    {
        var current = name;
        string previous;

        do
        {
            previous = current;
            current = current.Trim().Trim('"', '\'');

            if (current.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                current = current[..^4];
            }
        }
        while (current != previous);

        var separator = current.LastIndexOfAny(['\\', '/']);
        return separator >= 0 ? current[(separator + 1)..].Trim() : current;
    }
}
