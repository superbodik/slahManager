using SlahManager.ViewModels.Base;

namespace SlahManager.ViewModels.Items;

public sealed class SpecGroupViewModel : LocalizedObject
{
    private readonly string _titleKey;

    public SpecGroupViewModel(string titleKey, IReadOnlyList<SpecRowViewModel> rows)
    {
        _titleKey = titleKey;
        Rows = rows;
    }

    public string Title => Text.Get(_titleKey);

    public IReadOnlyList<SpecRowViewModel> Rows { get; }
}
