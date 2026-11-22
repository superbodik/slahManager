using System.Reflection;
using System.Windows.Input;
using SlahManager.Infrastructure;
using SlahManager.Localization;
using SlahManager.Services.Platform;
using SlahManager.Services.Settings;
using SlahManager.Services.Tweaks;
using SlahManager.ViewModels.Base;
using SlahManager.ViewModels.Items;

namespace SlahManager.ViewModels.Pages;

public sealed class SettingsViewModel : PageViewModel
{
    private readonly SettingsService _settings;
    private readonly TweakEngine _engine;
    private readonly IReadOnlyList<TweakItemViewModel> _items;
    private readonly DialogService _dialogs;
    private readonly ShellService _shell;
    private readonly StartupService _startup;
    private Func<string> _status = static () => string.Empty;
    private bool _isWorking;
    private bool _startWithWindows;

    public SettingsViewModel(
        SettingsService settings,
        TweakEngine engine,
        IReadOnlyList<TweakItemViewModel> items,
        DialogService dialogs,
        ShellService shell,
        StartupService startup)
        : base("nav.settings")
    {
        _settings = settings;
        _engine = engine;
        _items = items;
        _dialogs = dialogs;
        _shell = shell;
        _startup = startup;

        RevertAllCommand = new AsyncRelayCommand(RevertAllAsync, () => !_isWorking);
        OpenLogsCommand = new RelayCommand(() => _shell.OpenFolder(AppPaths.Logs));
    }

    public ICommand RevertAllCommand { get; }

    public ICommand OpenLogsCommand { get; }

    public string Version { get; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.2.0";

    public string DataPath => AppPaths.DataRoot;

    public bool IsEnglish
    {
        get => _settings.Language == AppLanguage.English;
        set
        {
            if (value)
            {
                _settings.Language = AppLanguage.English;
            }
        }
    }

    public bool IsRussian
    {
        get => _settings.Language == AppLanguage.Russian;
        set
        {
            if (value)
            {
                _settings.Language = AppLanguage.Russian;
            }
        }
    }

    public bool CreateRestorePoint
    {
        get => _settings.CreateRestorePoint;
        set
        {
            _settings.CreateRestorePoint = value;
            OnPropertyChanged();
        }
    }

    public bool RunInTray
    {
        get => _settings.RunInTray;
        set
        {
            _settings.RunInTray = value;
            OnPropertyChanged();
        }
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set
        {
            if (_startWithWindows == value)
            {
                return;
            }

            _startWithWindows = value;
            OnPropertyChanged();
            _ = ApplyStartupAsync(value);
        }
    }

    public string StatusMessage => _status();

    public bool HasStatus => StatusMessage.Length > 0;

    public async Task LoadStartupAsync()
    {
        _startWithWindows = await _startup.IsEnabledAsync();
        OnPropertyChanged(nameof(StartWithWindows));
    }

    private async Task ApplyStartupAsync(bool enabled)
    {
        var result = await _startup.SetEnabledAsync(enabled);
        if (result.Success)
        {
            return;
        }

        _startWithWindows = !enabled;
        OnPropertyChanged(nameof(StartWithWindows));
        SetStatus(() => Text.Format("settings.status.startupFailed", result.Error ?? string.Empty));
    }

    private void SetStatus(Func<string> builder)
    {
        _status = builder;
        OnPropertyChanged(nameof(StatusMessage));
        OnPropertyChanged(nameof(HasStatus));
    }

    private async Task RevertAllAsync()
    {
        var queue = _items
            .Where(item => item.IsApplied && item.CanToggle && _engine.HasBackup(item.Definition))
            .ToList();

        if (queue.Count == 0)
        {
            SetStatus(() => Text.Get("settings.status.nothing"));
            return;
        }

        if (!_dialogs.Confirm(Text.Format("settings.revertAll.confirm", queue.Count), Text.Get("settings.revertAll.title")))
        {
            return;
        }

        var reverted = 0;
        var failed = 0;

        _isWorking = true;
        try
        {
            for (var index = 0; index < queue.Count; index++)
            {
                var item = queue[index];
                var position = index + 1;
                SetStatus(() => Text.Format("settings.status.reverting", position, queue.Count, item.Name));

                var result = await item.SetAppliedAsync(false);
                if (result.Success)
                {
                    reverted++;
                }
                else
                {
                    failed++;
                }
            }
        }
        finally
        {
            _isWorking = false;
        }

        SetStatus(() => failed == 0
            ? Text.Format("settings.status.reverted", reverted)
            : Text.Format("settings.status.revertedFailed", reverted, failed));
    }
}
