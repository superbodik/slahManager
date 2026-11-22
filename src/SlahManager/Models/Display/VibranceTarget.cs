namespace SlahManager.Models.Display;

public sealed record VibranceTarget(DisplayVendor Vendor, string Key, int Raw, int Min, int Max, int Default)
{
    private const int NeutralPercent = 50;

    public int Percent => Default == 0 ? Raw : Raw * NeutralPercent / Default;

    public int RawFor(int percent) => Math.Clamp(percent * Default / NeutralPercent, Min, Max);
}
