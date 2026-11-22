using System.Windows.Input;
using SlahManager.Models.Hardware;
using SlahManager.Models.Profiles;
using SlahManager.Services.Platform;
using SlahManager.ViewModels.Base;
using SlahManager.ViewModels.Builders;
using SlahManager.ViewModels.Items;

namespace SlahManager.ViewModels.Pages;

public sealed class DashboardViewModel : PageViewModel
{
    private readonly SystemProfile _profile;
    private readonly IReadOnlyList<TweakItemViewModel> _items;
    private readonly ActivationService _activation;
    private readonly RestorePointService _restorePoints;
    private readonly RestorePointGuard _restorePointGuard;
    private readonly SpecRowViewModel _activationRow;
    private Func<string> _status = static () => string.Empty;
    private int _recommendedCount;
    private bool _isWorking;

    public DashboardViewModel(
        HardwareProfile hardware,
        SystemProfile profile,
        IReadOnlyList<TweakItemViewModel> items,
        ActivationService activation,
        RestorePointService restorePoints,
        RestorePointGuard restorePointGuard)
        : base("nav.overview")
    {
        _profile = profile;
        _items = items;
        _activation = activation;
        _restorePoints = restorePoints;
        _restorePointGuard = restorePointGuard;
        _activationRow = new SpecRowViewModel("spec.activation", () => Text.Get("activation.checking"));

        MachineLine = $"{hardware.MachineName}  ·  {hardware.Os.Name}";
        Groups = SpecGroupBuilder.Build(hardware, _activationRow);
        ApplyRecommendedCommand = new AsyncRelayCommand(ApplyRecommendedAsync, () => !IsWorking && RecommendedCount > 0);
        CreateRestorePointCommand = new AsyncRelayCommand(CreateRestorePointAsync, () => !IsWorking);

        foreach (var item in items)
        {
            item.StateChanged += (_, _) => RecomputeRecommended();
        }

        RecomputeRecommended();
    }

    public string MachineLine { get; }

    public string ProfileTitle => Text.Get(_profile.TitleKey);

    public string ProfileSummary => Text.Format(_profile.SummaryKey, _profile.SummaryArgument ?? string.Empty);

    public IReadOnlyList<SpecGroupViewModel> Groups { get; }

    public ICommand ApplyRecommendedCommand { get; }

    public ICommand CreateRestorePointCommand { get; }

    public int RecommendedCount
    {
        get => _recommendedCount;
        private set
        {
            if (SetProperty(ref _recommendedCount, value))
            {
                OnPropertyChanged(nameof(ApplyLabel));
            }
        }
    }

    public string ApplyLabel => RecommendedCount > 0
        ? Text.Format("dashboard.apply", RecommendedCount)
        : Text.Get("dashboard.nothing");

    public bool IsWorking
    {
        get => _isWorking;
        private set => SetProperty(ref _isWorking, value);
    }

    public string StatusMessage => _status();

    public bool HasStatus => StatusMessage.Length > 0;

    public async Task LoadActivationAsync()
    {
        var state = await _activation.GetStateAsync();
        var (key, tone) = state switch
        {
            ActivationState.Activated => ("activation.activated", SpecTone.Positive),
            ActivationState.NotActivated => ("activation.notActivated", SpecTone.Warning),
            _ => ("activation.unknown", SpecTone.Normal)
        };

        _activationRow.Update(() => Text.Get(key), tone);
    }

    private IEnumerable<TweakItemViewModel> RecommendedItems() => _items.Where(item =>
        item.Definition.Recommended
        && item.IsAvailable
        && !item.IsChecking
        && !item.IsApplied
        && (item.Definition.Profiles.Count == 0 || item.Definition.Profiles.Contains(_profile.Id)));

    private void RecomputeRecommended()
    {
        RecommendedCount = RecommendedItems().Count();
        CommandManager.InvalidateRequerySuggested();
    }

    private void SetStatus(Func<string> builder)
    {
        _status = builder;
        OnPropertyChanged(nameof(StatusMessage));
        OnPropertyChanged(nameof(HasStatus));
    }

    private async Task ApplyRecommendedAsync()
    {
        var queue = RecommendedItems().ToList();
        var applied = 0;
        var failed = 0;
        var reboot = false;

        IsWorking = true;
        try
        {
            if (_restorePointGuard.IsPending)
            {
                SetStatus(() => Text.Get("status.creatingRestorePoint"));
                await _restorePointGuard.EnsureAsync();
            }

            for (var index = 0; index < queue.Count; index++)
            {
                var item = queue[index];
                var position = index + 1;
                SetStatus(() => Text.Format("status.applying", position, queue.Count, item.Name));

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
        }
        finally
        {
            IsWorking = false;
        }

        SetStatus(() =>
        {
            var summary = failed == 0
                ? Text.Format("status.applied", applied)
                : Text.Format("status.appliedFailed", applied, failed);
            return reboot ? $"{summary}  ·  {Text.Get("status.restartRequired")}" : summary;
        });
    }

    private async Task CreateRestorePointAsync()
    {
        IsWorking = true;
        SetStatus(() => Text.Get("status.creatingRestorePoint"));

        try
        {
            var result = await _restorePoints.CreateAsync();
            SetStatus(() => result.Success
                ? Text.Get("status.restorePointCreated")
                : Text.Format("status.restorePointFailed", result.Error ?? string.Empty));
        }
        finally
        {
            IsWorking = false;
        }
    }
}
