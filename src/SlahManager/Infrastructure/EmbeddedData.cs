using System.Reflection;

namespace SlahManager.Infrastructure;

public static class EmbeddedData
{
    private static readonly Assembly Source = typeof(EmbeddedData).Assembly;

    public static IReadOnlyList<EmbeddedFile> ReadFolder(string folder)
    {
        var prefix = $"Data/{folder}/";

        return Source.GetManifestResourceNames()
            .Select(resource => new { Resource = resource, Path = Normalize(resource) })
            .Where(entry => entry.Path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase)
            .Select(entry => new EmbeddedFile(entry.Path, ReadResource(entry.Resource)))
            .ToList();
    }

    public static string? ReadText(string relativePath)
    {
        var wanted = $"Data/{relativePath}";
        var resource = Source.GetManifestResourceNames()
            .FirstOrDefault(name => string.Equals(Normalize(name), wanted, StringComparison.OrdinalIgnoreCase));

        return resource is null ? null : ReadResource(resource);
    }

    private static string Normalize(string resourceName) => resourceName.Replace('\\', '/');

    private static string ReadResource(string resource)
    {
        using var stream = Source.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Missing embedded resource {resource}.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
