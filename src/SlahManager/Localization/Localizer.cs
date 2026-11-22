using System.ComponentModel;
using System.Globalization;

namespace SlahManager.Localization;

public sealed class Localizer : INotifyPropertyChanged
{
    private const string IndexerName = "Item[]";

    private Dictionary<string, string> _fallback = new();
    private Dictionary<string, string> _current = new();

    public static Localizer Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? LanguageChanged;

    public AppLanguage Language { get; private set; } = AppLanguage.English;

    public string this[string key] => Get(key);

    public string Get(string key) => TryGet(key) ?? key;

    public string? TryGet(string key)
    {
        if (_current.TryGetValue(key, out var value))
        {
            return value;
        }

        return _fallback.TryGetValue(key, out var fallback) ? fallback : null;
    }

    public string Format(string key, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, Get(key), arguments);

    public void SetLanguage(AppLanguage language)
    {
        _fallback = StringTable.Load(AppLanguage.English);
        _current = language == AppLanguage.English ? _fallback : StringTable.Load(language);
        Language = language;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
