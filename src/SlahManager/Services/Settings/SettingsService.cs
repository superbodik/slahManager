using System.Globalization;
using SlahManager.Localization;
using SlahManager.Models.Boost;
using SlahManager.Models.Settings;
using SlahManager.Services.Boost;

namespace SlahManager.Services.Settings;

public sealed class SettingsService
{
    private readonly object _gate = new();
    private readonly SettingsStore _store;
    private readonly AppSettings _settings;

    public SettingsService(SettingsStore store)
    {
        _store = store;
        _settings = store.Load() ?? CreateDefault();

        var original = _settings.CustomGames ?? [];
        var cleaned = original
            .Select(GameLocations.NormalizeName)
            .Where(name => name.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name => $"{name}.exe")
            .ToList();

        _settings.CustomGames = cleaned;

        if (!original.SequenceEqual(cleaned))
        {
            Save();
        }
    }

    public AppLanguage Language
    {
        get => _settings.Language;
        set
        {
            if (_settings.Language == value)
            {
                return;
            }

            _settings.Language = value;
            Localizer.Instance.SetLanguage(value);
            Save();
        }
    }

    public bool CreateRestorePoint
    {
        get => _settings.CreateRestorePoint;
        set => Update(() => _settings.CreateRestorePoint == value, () => _settings.CreateRestorePoint = value);
    }

    public bool RunInTray
    {
        get => _settings.RunInTray;
        set => Update(() => _settings.RunInTray == value, () => _settings.RunInTray = value);
    }

    public bool GameBoostEnabled
    {
        get => _settings.GameBoostEnabled;
        set => Update(() => _settings.GameBoostEnabled == value, () => _settings.GameBoostEnabled = value);
    }

    public bool BoostTimerResolution
    {
        get => _settings.BoostTimerResolution;
        set => Update(() => _settings.BoostTimerResolution == value, () => _settings.BoostTimerResolution = value);
    }

    public bool BoostProcessPriority
    {
        get => _settings.BoostProcessPriority;
        set => Update(() => _settings.BoostProcessPriority == value, () => _settings.BoostProcessPriority = value);
    }

    public bool BoostCpuPower
    {
        get => _settings.BoostCpuPower;
        set => Update(() => _settings.BoostCpuPower == value, () => _settings.BoostCpuPower = value);
    }

    public bool BoostPauseWifiScan
    {
        get => _settings.BoostPauseWifiScan;
        set => Update(() => _settings.BoostPauseWifiScan == value, () => _settings.BoostPauseWifiScan = value);
    }

    public bool Cs2Vibrance
    {
        get => _settings.Cs2Vibrance;
        set => Update(() => _settings.Cs2Vibrance == value, () => _settings.Cs2Vibrance = value);
    }

    public int Cs2VibranceLevel
    {
        get => _settings.Cs2VibranceLevel;
        set => Update(() => _settings.Cs2VibranceLevel == value, () => _settings.Cs2VibranceLevel = value);
    }

    public IReadOnlyList<string> CustomGames
    {
        get
        {
            lock (_gate)
            {
                return _settings.CustomGames.ToList();
            }
        }
    }

    public BoostOptions BoostOptions => new(
        _settings.GameBoostEnabled,
        _settings.BoostTimerResolution,
        _settings.BoostProcessPriority,
        _settings.BoostCpuPower,
        _settings.BoostPauseWifiScan,
        _settings.Cs2Vibrance,
        _settings.Cs2VibranceLevel,
        CustomGames);

    public bool AddCustomGame(string name)
    {
        var normalized = GameLocations.NormalizeName(name);
        if (normalized.Length == 0)
        {
            return false;
        }

        lock (_gate)
        {
            if (_settings.CustomGames.Any(game => string.Equals(GameLocations.NormalizeName(game), normalized, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            _settings.CustomGames.Add($"{normalized}.exe");
        }

        Save();
        return true;
    }

    public void RemoveCustomGame(string name)
    {
        lock (_gate)
        {
            _settings.CustomGames.RemoveAll(game => string.Equals(game, name, StringComparison.OrdinalIgnoreCase));
        }

        Save();
    }

    public void ApplyLanguage() => Localizer.Instance.SetLanguage(_settings.Language);

    private void Update(Func<bool> isUnchanged, Action change)
    {
        if (isUnchanged())
        {
            return;
        }

        change();
        Save();
    }

    private void Save()
    {
        lock (_gate)
        {
            _store.Save(_settings);
        }
    }

    private static AppSettings CreateDefault() => new()
    {
        Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru"
            ? AppLanguage.Russian
            : AppLanguage.English
    };
}
