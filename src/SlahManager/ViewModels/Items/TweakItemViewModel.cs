using SlahManager.Models.Tweaks;
using SlahManager.Services.Platform;
using SlahManager.Services.Tweaks;
using SlahManager.ViewModels.Base;

namespace SlahManager.ViewModels.Items;

public sealed class TweakItemViewModel : LocalizedObject
{
    private readonly TweakEngine _engine;
    private readonly RestorePointGuard _restorePoints;
    private readonly Availability _availability;
    private bool _isApplied;
    private bool _isBusy;
    private bool _isChecking;
    private string? _errorKey;
    private string _errorRaw = string.Empty;

    public TweakItemViewModel(
        TweakDefinition definition,
        Availability availability,
        TweakEngine engine,
        RestorePointGuard restorePoints)
    {
        Definition = definition;
        _availability = availability;
        _engine = engine;
        _restorePoints = restorePoints;
        _isChecking = availability.IsAvailable;
        ToggleCommand = new AsyncRelayCommand(() => SetAppliedAsync(!IsApplied));
    }

    public event EventHandler? StateChanged;

    public TweakDefinition Definition { get; }

    public AsyncRelayCommand ToggleCommand { get; }

    public string Name => Text.TryGet($"tweak.{Definition.Id}.name") ?? Definition.Name;

    public string Description => Text.TryGet($"tweak.{Definition.Id}.description") ?? Definition.Description;

    public RiskLevel Risk => Definition.Risk;

    public string RiskLabel => Text.Get($"risk.{Definition.Risk.ToString().ToLowerInvariant()}");

    public bool RebootRequired => Definition.RebootRequired;

    public bool IsPermanent => !Definition.Reversible;

    public bool IsAvailable => _availability.IsAvailable;

    public bool HasUnavailableReason => !IsAvailable;

    public string UnavailableReason => _availability.ReasonKey is null
        ? string.Empty
        : Text.Format(_availability.ReasonKey, _availability.ReasonArgument ?? string.Empty);

    public bool IsApplied
    {
        get => _isApplied;
        private set => SetProperty(ref _isApplied, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RaiseDerived();
            }
        }
    }

    public bool IsChecking
    {
        get => _isChecking;
        private set
        {
            if (SetProperty(ref _isChecking, value))
            {
                RaiseDerived();
            }
        }
    }

    public string ErrorText => _errorKey is not null ? Text.Get(_errorKey) : _errorRaw;

    public bool HasError => ErrorText.Length > 0;

    public bool CanToggle => IsAvailable && !IsBusy && !IsChecking && (!IsApplied || Definition.Reversible);

    public string StatusText => IsChecking ? Text.Get("tweak.status.checking")
        : IsBusy ? Text.Get("tweak.status.working")
        : string.Empty;

    public async Task RefreshAsync()
    {
        if (!IsAvailable)
        {
            return;
        }

        IsChecking = true;
        IsApplied = await _engine.IsAppliedAsync(Definition);
        IsChecking = false;
        RaiseState();
    }

    public async Task<TweakResult> SetAppliedAsync(bool desired)
    {
        if (!CanToggle)
        {
            return TweakResult.FailKey("error.unavailable");
        }

        IsBusy = true;
        SetError(null, string.Empty);

        try
        {
            if (desired)
            {
                await _restorePoints.EnsureAsync();
            }

            var result = desired
                ? await _engine.ApplyAsync(Definition)
                : await _engine.RevertAsync(Definition);

            IsApplied = await _engine.IsAppliedAsync(Definition);

            if (!result.Success)
            {
                SetError(result.ErrorKey ?? (result.Error is null ? "error.failed" : null), result.Error ?? string.Empty);
            }
            else if (IsApplied != desired)
            {
                SetError("error.notAccepted", string.Empty);
            }

            return result;
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsApplied));
            RaiseState();
        }
    }

    private void SetError(string? key, string raw)
    {
        _errorKey = key;
        _errorRaw = raw;
        OnPropertyChanged(nameof(ErrorText));
        OnPropertyChanged(nameof(HasError));
    }

    private void RaiseDerived()
    {
        OnPropertyChanged(nameof(CanToggle));
        OnPropertyChanged(nameof(StatusText));
    }

    private void RaiseState()
    {
        RaiseDerived();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
