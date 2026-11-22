using System.Text.Json;
using SlahManager.Infrastructure;
using SlahManager.Models.Tweaks;

namespace SlahManager.Services.Tweaks;

public sealed class TweakCatalog
{
    public TweakCatalog()
    {
        Tweaks = Load();
    }

    public IReadOnlyList<TweakDefinition> Tweaks { get; }

    private static IReadOnlyList<TweakDefinition> Load()
    {
        var tweaks = new List<TweakDefinition>();
        var knownIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in EmbeddedData.ReadFolder("Tweaks"))
        {
            try
            {
                var definitions = JsonSerializer.Deserialize<List<TweakDefinition>>(file.Content, JsonSettings.Options) ?? [];
                tweaks.AddRange(definitions.Where(definition => knownIds.Add(definition.Id)));
            }
            catch (Exception exception)
            {
                Logger.Error($"Could not read {file.Path}", exception);
            }
        }

        return tweaks;
    }
}
