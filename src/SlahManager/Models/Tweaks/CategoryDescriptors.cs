namespace SlahManager.Models.Tweaks;

public static class CategoryDescriptors
{
    public static IReadOnlyList<CategoryDescriptor> All { get; } = Enum
        .GetValues<TweakCategory>()
        .Select(category =>
        {
            var key = category.ToString().ToLowerInvariant();
            return new CategoryDescriptor(category, $"category.{key}.title", $"category.{key}.subtitle");
        })
        .ToList();
}
