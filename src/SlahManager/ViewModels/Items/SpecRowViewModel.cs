using SlahManager.ViewModels.Base;

namespace SlahManager.ViewModels.Items;

public sealed class SpecRowViewModel : LocalizedObject
{
    private readonly string _labelKey;
    private Func<string> _value;
    private SpecTone _tone;

    public SpecRowViewModel(string labelKey, Func<string> value, SpecTone tone = SpecTone.Normal)
    {
        _labelKey = labelKey;
        _value = value;
        _tone = tone;
    }

    public string Label => Text.Get(_labelKey);

    public string Value => _value();

    public SpecTone Tone => _tone;

    public void Update(Func<string> value, SpecTone tone)
    {
        _value = value;
        _tone = tone;
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(Tone));
    }
}
