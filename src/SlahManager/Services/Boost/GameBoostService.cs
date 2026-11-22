using SlahManager.Infrastructure;
using SlahManager.Models.Boost;

namespace SlahManager.Services.Boost;

public sealed class GameBoostService : IDisposable
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ReleaseDelay = TimeSpan.FromSeconds(5);
    private const string Cs2Process = "cs2.exe";

    private readonly Func<BoostOptions> _options;
    private readonly IGameDetector _detector;
    private readonly TimerBooster _timer;
    private readonly ProcessBooster _processes;
    private readonly PowerBooster _power;
    private readonly WifiScanBooster _wifi;
    private readonly VibranceBooster _vibrance;
    private readonly object _gate = new();
    private readonly Timer _ticker;
    private SynchronizationContext? _context;
    private bool _active;
    private bool _powerBlocked;
    private bool _wifiBlocked;
    private bool _vibranceBlocked;
    private DateTime _lastSeenUtc;
    private string _optionsSignature = string.Empty;
    private string _stateSignature = string.Empty;
    private BoostState _state = BoostState.Idle;

    public GameBoostService(
        Func<BoostOptions> options,
        IGameDetector detector,
        TimerBooster timer,
        ProcessBooster processes,
        PowerBooster power,
        WifiScanBooster wifi,
        VibranceBooster vibrance)
    {
        _options = options;
        _detector = detector;
        _timer = timer;
        _processes = processes;
        _power = power;
        _wifi = wifi;
        _vibrance = vibrance;
        _ticker = new Timer(_ => Tick(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public event EventHandler? StateChanged;

    public BoostState State
    {
        get
        {
            lock (_gate)
            {
                return _state;
            }
        }
    }

    public void Start()
    {
        _context = SynchronizationContext.Current;

        lock (_gate)
        {
            _power.Restore();
            _wifi.Restore();
            _vibrance.Restore();
        }

        _ticker.Change(TimeSpan.Zero, TickInterval);
    }

    public void Stop()
    {
        _ticker.Change(Timeout.Infinite, Timeout.Infinite);

        lock (_gate)
        {
            Deactivate();
            Publish(BoostState.Idle);
        }
    }

    public void Tick()
    {
        if (!Monitor.TryEnter(_gate))
        {
            return;
        }

        try
        {
            var options = _options();
            ApplyOptionChanges(options);

            if (!options.Enabled)
            {
                Deactivate();
                Publish(BoostState.Idle);
                return;
            }

            var games = _detector.Scan(options.CustomGames);

            if (games.Count > 0)
            {
                _lastSeenUtc = DateTime.UtcNow;

                if (!_active)
                {
                    Activate(options);
                }

                if (options.ProcessPriority)
                {
                    _processes.Apply(games);
                }

                UpdateVibrance(options, games);
            }
            else if (_active && DateTime.UtcNow - _lastSeenUtc > ReleaseDelay)
            {
                Deactivate();
            }

            Publish(Describe(games));
        }
        catch (Exception exception)
        {
            Logger.Error("Game boost tick failed", exception);
        }
        finally
        {
            Monitor.Exit(_gate);
        }
    }

    public void Dispose()
    {
        Stop();
        _ticker.Dispose();
    }

    private void ApplyOptionChanges(BoostOptions options)
    {
        if (options.Signature == _optionsSignature)
        {
            return;
        }

        _optionsSignature = options.Signature;

        if (_active)
        {
            Deactivate();
        }
    }

    private void Activate(BoostOptions options)
    {
        if (options.TimerResolution)
        {
            _timer.Acquire();
        }

        _powerBlocked = options.CpuPower && !TryRun("CPU power profile", _power.Apply);
        _wifiBlocked = options.PauseWifiScan && !TryRun("Wi-Fi scan pause", () =>
        {
            if (!_wifi.Apply())
            {
                throw new InvalidOperationException("Windows refused to pause Wi-Fi scanning.");
            }
        });

        _active = true;
        Logger.Info("Game boost activated");
    }

    private void Deactivate()
    {
        if (!_active && !_timer.IsHeld && !_power.IsApplied && !_wifi.IsApplied && !_vibrance.IsApplied)
        {
            return;
        }

        _timer.Release();
        _processes.RestoreAll();
        TryRun("CPU power profile restore", _power.Restore);
        TryRun("Wi-Fi scan restore", _wifi.Restore);
        TryRun("Vibrance restore", _vibrance.Restore);

        _powerBlocked = false;
        _wifiBlocked = false;
        _vibranceBlocked = false;
        _active = false;
        Logger.Info("Game boost deactivated");
    }

    private void UpdateVibrance(BoostOptions options, IReadOnlyList<GameProcess> games)
    {
        var wanted = options.Cs2Vibrance && games.Any(game => string.Equals(game.Name, Cs2Process, StringComparison.OrdinalIgnoreCase));

        if (!wanted)
        {
            _vibranceBlocked = false;

            if (_vibrance.IsApplied)
            {
                TryRun("Vibrance restore", _vibrance.Restore);
            }

            return;
        }

        if (_vibranceBlocked)
        {
            return;
        }

        _vibranceBlocked = !TryRun("Vibrance", () =>
        {
            if (!_vibrance.Apply(options.VibranceLevel))
            {
                throw new InvalidOperationException("No display accepted the vibrance change.");
            }
        });
    }

    private static bool TryRun(string action, Action work)
    {
        try
        {
            work();
            return true;
        }
        catch (Exception exception)
        {
            Logger.Error($"{action} failed", exception);
            return false;
        }
    }

    private BoostState Describe(IReadOnlyList<GameProcess> games) => new(
        _active,
        games,
        _active && _timer.IsHeld ? _timer.CurrentMilliseconds : null,
        _processes.AnyApplied,
        _power.IsApplied,
        _powerBlocked,
        _wifi.IsApplied,
        _wifiBlocked,
        _vibrance.AppliedPercent,
        _vibranceBlocked);

    private void Publish(BoostState state)
    {
        if (state.Signature == _stateSignature)
        {
            return;
        }

        _stateSignature = state.Signature;
        _state = state;

        if (_context is null)
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        _context.Post(_ => StateChanged?.Invoke(this, EventArgs.Empty), null);
    }
}
