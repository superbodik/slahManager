using SlahManager.Localization;

namespace SlahManager.ViewModels.Base;

public abstract class LocalizedObject : ObservableObject
{
    protected LocalizedObject()
    {
        Localizer.Instance.LanguageChanged += (_, _) => OnPropertyChanged(string.Empty);
    }

    protected static Localizer Text => Localizer.Instance;
}
