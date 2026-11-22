using SlahManager.ViewModels.Base;

namespace SlahManager.ViewModels.Pages;

public abstract class PageViewModel : LocalizedObject
{
    private readonly string _titleKey;
    private string _badge = string.Empty;

    protected PageViewModel(string titleKey)
    {
        _titleKey = titleKey;
    }

    public string Title => Text.Get(_titleKey);

    public string Badge
    {
        get => _badge;
        protected set => SetProperty(ref _badge, value);
    }
}
