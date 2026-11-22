using SlahManager.Infrastructure;
using SlahManager.Models.Backup;
using SlahManager.Models.Tweaks;
using SlahManager.Services.Tweaks.Executors;

namespace SlahManager.Services.Tweaks;

public sealed class TweakEngine
{
    private readonly OperationExecutorRegistry _executors;
    private readonly BackupStore _backups;

    public TweakEngine(OperationExecutorRegistry executors, BackupStore backups)
    {
        _executors = executors;
        _backups = backups;
    }

    public bool HasBackup(TweakDefinition tweak) => _backups.Exists(tweak.Id);

    public Task<bool> IsAppliedAsync(TweakDefinition tweak) => Task.Run(() => IsApplied(tweak));

    public Task<TweakResult> ApplyAsync(TweakDefinition tweak) => Task.Run(() => Apply(tweak));

    public Task<TweakResult> RevertAsync(TweakDefinition tweak) => Task.Run(() => Revert(tweak));

    private bool IsApplied(TweakDefinition tweak)
    {
        try
        {
            return tweak.Operations.All(operation => _executors.Resolve(operation).IsApplied(operation));
        }
        catch (Exception exception)
        {
            Logger.Error($"State check failed for {tweak.Id}", exception);
            return false;
        }
    }

    private TweakResult Apply(TweakDefinition tweak)
    {
        var snapshots = new List<OperationSnapshot>();
        var createdBackup = false;

        try
        {
            foreach (var operation in tweak.Operations)
            {
                snapshots.Add(_executors.Resolve(operation).Capture(operation));
            }

            var existing = _backups.Load(tweak.Id);
            if (existing is null || existing.Snapshots.Count != snapshots.Count)
            {
                _backups.Save(new TweakBackup { TweakId = tweak.Id, CreatedUtc = DateTime.UtcNow, Snapshots = snapshots });
                createdBackup = true;
            }

            foreach (var operation in tweak.Operations)
            {
                _executors.Resolve(operation).Apply(operation);
            }

            Logger.Info($"Applied {tweak.Id}");
            return TweakResult.Ok();
        }
        catch (Exception exception)
        {
            Logger.Error($"Apply failed for {tweak.Id}", exception);
            Rollback(tweak, snapshots);

            if (createdBackup)
            {
                _backups.Delete(tweak.Id);
            }

            return TweakResult.Fail(exception.Message);
        }
    }

    private TweakResult Revert(TweakDefinition tweak)
    {
        if (!tweak.Reversible)
        {
            return TweakResult.FailKey("error.permanent");
        }

        try
        {
            var backup = _backups.Load(tweak.Id);
            var usable = backup is not null && backup.Snapshots.Count == tweak.Operations.Count;

            for (var index = 0; index < tweak.Operations.Count; index++)
            {
                var operation = tweak.Operations[index];
                _executors.Resolve(operation).Restore(operation, usable ? backup!.Snapshots[index] : null);
            }

            _backups.Delete(tweak.Id);
            Logger.Info($"Reverted {tweak.Id}");
            return TweakResult.Ok();
        }
        catch (Exception exception)
        {
            Logger.Error($"Revert failed for {tweak.Id}", exception);
            return TweakResult.Fail(exception.Message);
        }
    }

    private void Rollback(TweakDefinition tweak, IReadOnlyList<OperationSnapshot> snapshots)
    {
        for (var index = 0; index < snapshots.Count; index++)
        {
            try
            {
                var operation = tweak.Operations[index];
                _executors.Resolve(operation).Restore(operation, snapshots[index]);
            }
            catch (Exception exception)
            {
                Logger.Error($"Rollback step failed for {tweak.Id}", exception);
            }
        }
    }
}
