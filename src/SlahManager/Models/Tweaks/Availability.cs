namespace SlahManager.Models.Tweaks;

public readonly record struct Availability(bool IsAvailable, string? ReasonKey, string? ReasonArgument)
{
    public static Availability Yes { get; } = new(true, null, null);

    public static Availability No(string reasonKey, string? argument = null) => new(false, reasonKey, argument);
}
