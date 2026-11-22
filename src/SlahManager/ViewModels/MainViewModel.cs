using System.Collections.ObjectModel;
using System.Windows.Input;
using SlahManager.Models.Tweaks;
using SlahManager.Services;
using SlahManager.Services.Platform;
using SlahManager.ViewModels.Base;
using SlahManager.ViewModels.Items;
using SlahManager.ViewModels.Pages;

namespace SlahManager.ViewModels;

public sealed class MainViewModel : LocalizedObject
{
    private const int MaxParallelChecks = 4;

    private readonly AppServices _services;
    private PageViewModel? _currentPage;
    private bool _isLoading = true;

    public MainViewModel(AppServices services)
    {
        _services = services;
        RestartElevatedCommand = new RelayCommand(ElevationService.RestartElevated);
    }

    public ObservableCollection<PageViewModel> Pages { get; } = [];

    public ICommand RestartElevatedCommand { get; }

    public bool ShowElevationNotice => !ElevationService.IsElevated;

    public string PrivilegeLabel => Text.Get(ElevationService.IsElevated ? "privilege.admin" : "privilege.user");

    public string MachineName => Environment.MachineName.ToUpperInvariant();

    public string Version { get; } =
        System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(2) ?? string.Empty;

    public PageViewModel? CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(IsReady));
            }
        }
    }

    public bool IsReady => !IsLoading;

    public async Task InitializeAsync()
    {
        var hardware = await Task.Run(() => _services.Scanner.Scan());
        var profile = _services.Profiles.Detect(hardware);

        var items = _services.Catalog.Tweaks
            .Select(tweak => new TweakItemViewModel(
                tweak,
                _services.Requirements.Evaluate(tweak.Requirements, hardware),
                _services.Engine,
                _services.RestorePointGuard))
            .ToList();

        var dashboard = new DashboardViewModel(
            hardware,
            profile,
            items,
            _services.Activation,
            _services.RestorePoints,
            _services.RestorePointGuard);

        Pages.Add(dashboard);

        Pages.Add(new GameBoostViewModel(
            _services.Settings,
            _services.Boost,
            items,
            _services.RestorePointGuard,
            _services.Ping,
            _services.Vibrance));

        foreach (var descriptor in CategoryDescriptors.All)
        {
            var group = items.Where(item => item.Definition.Category == descriptor.Category).ToList();
            if (group.Count > 0)
            {
                Pages.Add(new TweakPageViewModel(descriptor, group));
            }
        }

        var settingsPage = new SettingsViewModel(
            _services.Settings,
            _services.Engine,
            items,
            _services.Dialogs,
            _services.Shell,
            _services.Startup);

        Pages.Add(settingsPage);

        CurrentPage = dashboard;
        IsLoading = false;

        await Task.WhenAll(
            dashboard.LoadActivationAsync(),
            settingsPage.LoadStartupAsync(),
            RefreshAsync(items));
    }

    private static async Task RefreshAsync(IEnumerable<TweakItemViewModel> items)
    {
        using var gate = new SemaphoreSlim(MaxParallelChecks);

        await Task.WhenAll(items.Select(async item =>
        {
            await gate.WaitAsync();
            try
            {
                await item.RefreshAsync();
            }
            finally
            {
                gate.Release();
            }
        }));
    }
}
