namespace SlahManager.Models.Tweaks;

public sealed record TweakResult(bool Success, string? Error = null, string? ErrorKey = null)
{
    public static TweakResult Ok() => new(true);

    public static TweakResult Fail(string error) => new(false, error);

    public static TweakResult FailKey(string errorKey) => new(false, null, errorKey);
}
