using SlahManager.ViewModels.Base;

namespace SlahManager.ViewModels.Items;

public sealed class ChoiceOption : ObservableObject
{
    private readonly Action<int> _select;
    private bool _isSelected;

    public ChoiceOption(int value, string label, bool isSelected, Action<int> select)
    {
        Value = value;
        Label = label;
        _isSelected = isSelected;
        _select = select;
    }

    public int Value { get; }

    public string Label { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value) && value)
            {
                _select(Value);
            }
        }
    }

    public void Sync(int selected)
    {
        _isSelected = selected == Value;
        OnPropertyChanged(nameof(IsSelected));
    }
}
