using SlahManager.Models.Boost;

namespace SlahManager.Services.Boost;

public interface IGameDetector
{
    IReadOnlyList<GameProcess> Scan(IReadOnlyCollection<string> customNames);
}
