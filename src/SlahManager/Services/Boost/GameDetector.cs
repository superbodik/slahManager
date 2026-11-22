using System.Diagnostics;
using SlahManager.Infrastructure;
using SlahManager.Infrastructure.Native;
using SlahManager.Models.Boost;

namespace SlahManager.Services.Boost;

public sealed class GameDetector : IGameDetector
{
    public IReadOnlyList<GameProcess> Scan(IReadOnlyCollection<string> customNames)
    {
        var custom = new HashSet<string>(customNames.Select(GameLocations.NormalizeName), StringComparer.OrdinalIgnoreCase);
        var games = new List<GameProcess>();

        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                var game = Inspect(process, custom);
                if (game is not null)
                {
                    games.Add(game);
                }
            }
        }

        return games;
    }

    private static GameProcess? Inspect(Process process, HashSet<string> custom)
    {
        try
        {
            if (process.Id <= 4)
            {
                return null;
            }

            var name = process.ProcessName;
            using var handle = ProcessHandle.OpenForQuery(process.Id);
            var path = handle?.ImagePath() ?? string.Empty;

            if (custom.Contains(name))
            {
                return new GameProcess(process.Id, $"{name}.exe", path);
            }

            var isCandidate = path.Length > 0
                && GameLocations.IsLibraryPath(path)
                && !GameLocations.IsExcludedName(name)
                && process.MainWindowHandle != IntPtr.Zero;

            return isCandidate ? new GameProcess(process.Id, $"{name}.exe", path) : null;
        }
        catch (Exception exception)
        {
            Logger.Error($"Could not inspect process {process.Id}", exception);
            return null;
        }
    }
}
