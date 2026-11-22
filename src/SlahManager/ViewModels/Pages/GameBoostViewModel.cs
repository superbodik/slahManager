using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using SlahManager.Models.Boost;
using SlahManager.Models.Display;
using SlahManager.Models.Network;
using SlahManager.Models.Tweaks;
using SlahManager.Services.Boost;
using SlahManager.Services.Display;
using SlahManager.Services.Network;
using SlahManager.Services.Platform;
using SlahManager.Services.Settings;
using SlahManager.ViewModels.Base;
using SlahManager.ViewModels.Items;

namespace SlahManager.ViewModels.Pages;

public sealed class GameBoostViewModel : PageViewModel
{
    private static readonly int[] VibranceLevels = [60, 70, 80, 90, 100];
    private const double RouterJitterLimit = 4;
    private const double RouterLatencyLimit = 8;

    private readonly SettingsService _settings;
    private readonly GameBoostService _boost;
    private readonly IReadOnlyList<TweakItemViewModel> _fixItems;
    private readonly RestorePointGuard _restorePoints;
    private readonly PingTester _pingTester;
    private readonly VibranceController _vibrance;
    private Func<string> _fixStatus = static () => string.Empty;
    private bool _isFixing;
    private bool _isPinging;
    private PingReport? _pingReport;
    private int? _screenVibrance;
    private string _newGameName = string.Empty;

    public GameBoostViewModel(
        SettingsService settings,
        GameBoostService boost,
        IReadOnlyList<TweakItemViewModel> items,
        RestorePointGuard restorePoints,
        PingTester pingTester,
        VibranceController vibrance)
        : base("nav.boost")
    {
        _settings = settings;
        _boost = boost;
        _restorePoints = restorePoints;
        _pingTester = pingTester;
        _vibrance = vibrance;
        _fixItems = items
            .Where(item => item.Definition.Category is TweakCategory.Input or TweakCategory.Network)
            .ToList();

        CustomGames = new ObservableCollection<string>(settings.CustomGames);
        VibranceOptions = VibranceLevels
            .Select(level => new ChoiceOption(level, $"{level}%", level == settings.Cs2VibranceLevel, SelectVibrance))
            .ToList();

        FixCommand = new AsyncRelayCommand(FixAsync, () => !_isFixing);
        PingCommand = new AsyncRelayCommand(PingAsync, () => !_isPinging);
        AddGameCommand = new RelayCommand(AddGame, () => NewGameName.Trim().Length > 0);
        RemoveGameCommand = new ParameterizedCommand<string>(RemoveGame);

        _boost.StateChanged += (_, _) => RaiseLiveState();

        foreach (var item in _fixItems)
        {
            item.StateChanged += (_, _) => OnPropertyChanged(nameof(ShowTimerHint));
        }

        _ = LoadScreenVibranceAsync();
    }

    public ObservableCollection<string> CustomGames { get; }

    public IReadOnlyList<ChoiceOption> VibranceOptions { get; }

    public ICommand FixCommand { get; }

    public ICommand PingCommand { get; }

    public ICommand AddGameCommand { get; }

    public ICommand RemoveGameCommand { get; }

    public bool IsEnabled
    {
        get => _settings.GameBoostEnabled;
        set => ChangeOption(() => _settings.GameBoostEnabled = value);
    }

    public bool BoostTimer
    {
        get => _settings.BoostTimerResolution;
        set => ChangeOption(() => _settings.BoostTimerResolution = value);
    }

    public bool BoostPriority
    {
        get => _settings.BoostProcessPriority;
        set => ChangeOption(() => _settings.BoostProcessPriority = value);
    }

    public bool BoostPower
    {
        get => _settings.BoostCpuPower;
        set => ChangeOption(() => _settings.BoostCpuPower = value);
    }

    public bool BoostWifi
    {
        get => _settings.BoostPauseWifiScan;
        set => ChangeOption(() => _settings.BoostPauseWifiScan = value);
    }

    public bool Cs2Vibrance
    {
        get => _settings.Cs2Vibrance;
        set => ChangeOption(() => _settings.Cs2Vibrance = value);
    }

    public bool HasVibranceSupport => _screenVibrance is not null;

    public string VibranceNow => _screenVibrance is { } percent
        ? Text.Format("boost.vibrance.now", percent)
        : Text.Get("boost.vibrance.unsupported");

    public string NewGameName
    {
        get => _newGameName;
        set => SetProperty(ref _newGameName, value);
    }

    public bool HasCustomGames => CustomGames.Count > 0;

    public bool ShowTimerHint => IsEnabled && BoostTimer && !TimerPolicy.IsSystemWide();

    public bool ShowPowerHint => IsEnabled && BoostPower && _boost.State is { IsActive: true, PowerBlocked: true };

    public string FixStatusMessage => _fixStatus();

    public bool HasFixStatus => FixStatusMessage.Length > 0;

    public bool HasPingResult => _pingReport is not null;

    public string PingButtonLabel => Text.Get(_isPinging ? "ping.running" : "ping.button");

    public string PingConnection => _pingReport?.Connection switch
    {
        ConnectionKind.WiFi => Text.Get("ping.connection.wifi"),
        ConnectionKind.Cable => Text.Get("ping.connection.cable"),
        ConnectionKind.Other => Text.Get("ping.connection.other"),
        _ => Text.Get("ping.connection.none")
    };

    public string PingRouter => DescribePing("ping.router", _pingReport?.Router);

    public string PingInternet => DescribePing("ping.internet", _pingReport?.Internet);

    public string PingAdvice
    {
        get
        {
            if (_pingReport is not { } report)
            {
                return string.Empty;
            }

            if (report.Router is { AverageMilliseconds: { } average, JitterMilliseconds: { } jitter }
                && (jitter > RouterJitterLimit || average > RouterLatencyLimit || report.Router.LossPercent > 0))
            {
                return report.Connection == ConnectionKind.WiFi
                    ? Text.Get("ping.advice.wifiUnstable")
                    : Text.Get("ping.advice.localUnstable");
            }

            return report.Connection == ConnectionKind.WiFi
                ? Text.Get("ping.advice.wifi")
                : Text.Get("ping.advice.good");
        }
    }

    public string StatusTitle
    {
        get
        {
            var state = _boost.State;

            if (!IsEnabled)
            {
                return Text.Get("boost.status.off");
            }

            if (!state.IsActive)
            {
                return Text.Get("boost.status.waiting");
            }

            return state.Games.Count == 0
                ? Text.Get("boost.status.restoring")
                : Text.Format("boost.status.active", string.Join(", ", state.Games.Select(game => game.Name).Distinct().Take(3)));
        }
    }

    public string StatusDetail
    {
        get
        {
            var state = _boost.State;

            if (!IsEnabled)
            {
                return Text.Get("boost.hint.off");
            }

            return state.IsActive ? DescribeActive(state) : Text.Get("boost.hint.waiting");
        }
    }

    private static string DescribePing(string key, PingStats? stats)
    {
        if (stats is null)
        {
            return string.Empty;
        }

        if (stats is not { AverageMilliseconds: { } average, JitterMilliseconds: { } jitter })
        {
            return Text.Format($"{key}.noReply", stats.Target);
        }

        return Text.Format(
            key,
            average.ToString("0", CultureInfo.CurrentCulture),
            jitter.ToString("0", CultureInfo.CurrentCulture),
            stats.LossPercent.ToString("0", CultureInfo.CurrentCulture));
    }

    private static string DescribeActive(BoostState state)
    {
        var parts = new List<string>();

        if (state.TimerMilliseconds is { } milliseconds)
        {
            parts.Add(Text.Format("boost.live.timer", milliseconds.ToString("0.00", CultureInfo.CurrentCulture)));
        }

        if (state.PriorityApplied)
        {
            parts.Add(Text.Get("boost.live.priority"));
        }

        if (state.PowerApplied)
        {
            parts.Add(Text.Get("boost.live.power"));
        }
        else if (state.PowerBlocked)
        {
            parts.Add(Text.Get("boost.live.powerBlocked"));
        }

        if (state.WifiPaused)
        {
            parts.Add(Text.Get("boost.live.wifi"));
        }
        else if (state.WifiBlocked)
        {
            parts.Add(Text.Get("boost.live.wifiBlocked"));
        }

        if (state.VibrancePercent is { } vibrance)
        {
            parts.Add(Text.Format("boost.live.vibrance", vibrance));
        }
        else if (state.VibranceBlocked)
        {
            parts.Add(Text.Get("boost.live.vibranceBlocked"));
        }

        return parts.Count == 0 ? Text.Get("boost.live.none") : string.Join("  ·  ", parts);
    }

    private async Task LoadScreenVibranceAsync()
    {
        var targets = await Task.Run(_vibrance.Read);
        _screenVibrance = targets
            .OrderBy(target => target.Vendor == DisplayVendor.Nvidia ? 0 : 1)
            .Select(target => (int?)target.Percent)
            .FirstOrDefault();

        OnPropertyChanged(nameof(HasVibranceSupport));
        OnPropertyChanged(nameof(VibranceNow));
    }

    private void SelectVibrance(int level)
    {
        _settings.Cs2VibranceLevel = level;

        foreach (var option in VibranceOptions)
        {
            option.Sync(level);
        }

        Task.Run(_boost.Tick);
    }

    private void ChangeOption(Action change)
    {
        change();
        RaiseLiveState();
        OnPropertyChanged(nameof(IsEnabled));
        OnPropertyChanged(nameof(BoostTimer));
        OnPropertyChanged(nameof(BoostPriority));
        OnPropertyChanged(nameof(BoostPower));
        OnPropertyChanged(nameof(BoostWifi));
        OnPropertyChanged(nameof(Cs2Vibrance));
        Task.Run(_boost.Tick);
    }

    private void RaiseLiveState()
    {
        OnPropertyChanged(nameof(StatusTitle));
        OnPropertyChanged(nameof(StatusDetail));
        OnPropertyChanged(nameof(ShowTimerHint));
        OnPropertyChanged(nameof(ShowPowerHint));
    }

    private void SetFixStatus(Func<string> builder)
    {
        _fixStatus = builder;
        OnPropertyChanged(nameof(FixStatusMessage));
        OnPropertyChanged(nameof(HasFixStatus));
    }

    private void AddGame()
    {
        _settings.AddCustomGame(NewGameName);
        NewGameName = string.Empty;
        ReloadGames();
    }

    private void RemoveGame(string name)
    {
        _settings.RemoveCustomGame(name);
        ReloadGames();
    }

    private void ReloadGames()
    {
        CustomGames.Clear();

        foreach (var game in _settings.CustomGames)
        {
            CustomGames.Add(game);
        }

        OnPropertyChanged(nameof(HasCustomGames));
        Task.Run(_boost.Tick);
    }

    private async Task PingAsync()
    {
        _isPinging = true;
        OnPropertyChanged(nameof(PingButtonLabel));

        try
        {
            _pingReport = await _pingTester.RunAsync();
        }
        finally
        {
            _isPinging = false;
            OnPropertyChanged(nameof(PingButtonLabel));
            OnPropertyChanged(nameof(HasPingResult));
            OnPropertyChanged(nameof(PingConnection));
            OnPropertyChanged(nameof(PingRouter));
            OnPropertyChanged(nameof(PingInternet));
            OnPropertyChanged(nameof(PingAdvice));
        }
    }

    private async Task FixAsync()
    {
        var queue = _fixItems
            .Where(item => item.Definition.Recommended && item.IsAvailable && !item.IsChecking && !item.IsApplied)
            .ToList();

        var applied = 0;
        var failed = 0;
        var reboot = false;

        _isFixing = true;
        try
        {
            if (queue.Count > 0 && _restorePoints.IsPending)
            {
                SetFixStatus(() => Text.Get("status.creatingRestorePoint"));
                await _restorePoints.EnsureAsync();
            }

            for (var index = 0; index < queue.Count; index++)
            {
                var item = queue[index];
                var position = index + 1;
                SetFixStatus(() => Text.Format("status.applying", position, queue.Count, item.Name));

                var result = await item.SetAppliedAsync(true);
                if (result.Success)
                {
                    applied++;
                    reboot |= item.RebootRequired;
                }
                else
                {
                    failed++;
                }
            }

            EnableBoost();
        }
        finally
        {
            _isFixing = false;
        }

        SetFixStatus(() => DescribeFix(queue.Count, applied, failed, reboot));
    }

    private void EnableBoost()
    {
        _settings.GameBoostEnabled = true;
        _settings.BoostTimerResolution = true;
        _settings.BoostProcessPriority = true;
        _settings.BoostCpuPower = true;
        _settings.BoostPauseWifiScan = true;
        ChangeOption(static () => { });
    }

    private static string DescribeFix(int queued, int applied, int failed, bool reboot)
    {
        if (queued == 0)
        {
            return Text.Get("boost.fix.nothing");
        }

        var summary = failed == 0
            ? Text.Format("boost.fix.done", applied)
            : Text.Format("boost.fix.doneFailed", applied, failed);

        return reboot ? $"{summary}  ·  {Text.Get("status.restartRequired")}" : summary;
    }
}
