using SlahManager.Models.Tweaks.Operations;

namespace SlahManager.Models.Tweaks;

public sealed class TweakDefinition
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public TweakCategory Category { get; init; }

    public RiskLevel Risk { get; init; } = RiskLevel.Low;

    public bool RebootRequired { get; init; }

    public bool Reversible { get; init; } = true;

    public bool Recommended { get; init; }

    public List<string> Profiles { get; init; } = [];

    public TweakRequirements Requirements { get; init; } = new();

    public List<OperationDefinition> Operations { get; init; } = [];
}
