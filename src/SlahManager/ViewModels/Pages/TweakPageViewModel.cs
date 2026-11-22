using SlahManager.Models.Tweaks;
using SlahManager.ViewModels.Items;

namespace SlahManager.ViewModels.Pages;

public sealed class TweakPageViewModel : PageViewModel
{
    private readonly string _subtitleKey;
    private int _applied;
    private int _available;

    public TweakPageViewModel(CategoryDescriptor descriptor, IReadOnlyList<TweakItemViewModel> items)
        : base(descriptor.TitleKey)
    {
        _subtitleKey = descriptor.SubtitleKey;
        Items = items;

        foreach (var item in items)
        {
            item.StateChanged += (_, _) => Refresh();
        }

        Refresh();
    }

    public string Subtitle => Text.Get(_subtitleKey);

    public IReadOnlyList<TweakItemViewModel> Items { get; }

    public string Summary => Text.Format("tweak.applied.summary", _applied, _available);

    private void Refresh()
    {
        _available = Items.Count(item => item.IsAvailable);
        _applied = Items.Count(item => item.IsAvailable && item.IsApplied);
        Badge = $"{_applied}/{_available}";
        OnPropertyChanged(nameof(Summary));
    }
}
