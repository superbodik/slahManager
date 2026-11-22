using System.Diagnostics;
using SlahManager.Infrastructure;
using SlahManager.Infrastructure.Native;
using SlahManager.Models.Boost;

namespace SlahManager.Services.Boost;

public sealed class ProcessBooster
{
    private readonly Dictionary<int, ProcessPriorityClass?> _changed = new();

    public bool AnyApplied => _changed.Values.Any(original => original is not null);

    public void Apply(IReadOnlyList<GameProcess> games)
    {
        var alive = games.Select(game => game.ProcessId).ToHashSet();

        foreach (var processId in _changed.Keys.Where(id => !alive.Contains(id)).ToList())
        {
            _changed.Remove(processId);
        }

        foreach (var game in games.Where(game => !_changed.ContainsKey(game.ProcessId)))
        {
            Boost(game);
        }
    }

    public void RestoreAll()
    {
        foreach (var (processId, original) in _changed)
        {
            if (original is not { } priority)
            {
                continue;
            }

            try
            {
                using var process = Process.GetProcessById(processId);
                process.PriorityClass = priority;
            }
            catch (Exception exception) when (HasExited(exception))
            {
                Logger.Info($"Process {processId} had already closed");
            }
            catch (Exception exception)
            {
                Logger.Error($"Could not restore priority of process {processId}", exception);
            }
        }

        _changed.Clear();
    }

    private static bool HasExited(Exception exception) =>
        exception is ArgumentException or InvalidOperationException;

    private void Boost(GameProcess game)
    {
        ProcessPriorityClass? original = null;

        try
        {
            using var process = Process.GetProcessById(game.ProcessId);
            var current = process.PriorityClass;

            if (current is ProcessPriorityClass.Normal or ProcessPriorityClass.BelowNormal or ProcessPriorityClass.Idle)
            {
                process.PriorityClass = ProcessPriorityClass.High;
                original = current;
            }
        }
        catch (Exception exception) when (HasExited(exception))
        {
            Logger.Info($"{game.Name} closed before it could be boosted");
        }
        catch (Exception exception)
        {
            Logger.Error($"Could not raise priority of {game.Name}", exception);
        }

        _changed[game.ProcessId] = original;

        using var handle = ProcessHandle.OpenForModify(game.ProcessId);
        if (handle is null || !handle.DisablePowerThrottling())
        {
            Logger.Info($"Power throttling could not be disabled for {game.Name}");
        }
    }
}
