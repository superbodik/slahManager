namespace SlahManager.Services.Tweaks.Executors;

internal static class RegistryFlagString
{
    public static string? Get(string text, string flag) =>
        Parse(text).FirstOrDefault(pair => IsFlag(pair, flag)).Value;

    public static string With(string text, string flag, string value)
    {
        var pairs = Parse(text);
        var replaced = false;

        for (var index = 0; index < pairs.Count; index++)
        {
            if (IsFlag(pairs[index], flag))
            {
                pairs[index] = new KeyValuePair<string, string>(pairs[index].Key, value);
                replaced = true;
            }
        }

        if (!replaced)
        {
            pairs.Add(new KeyValuePair<string, string>(flag, value));
        }

        return Join(pairs);
    }

    public static string Without(string text, string flag)
    {
        var pairs = Parse(text);
        pairs.RemoveAll(pair => IsFlag(pair, flag));
        return Join(pairs);
    }

    private static bool IsFlag(KeyValuePair<string, string> pair, string flag) =>
        string.Equals(pair.Key, flag, StringComparison.OrdinalIgnoreCase);

    private static List<KeyValuePair<string, string>> Parse(string text)
    {
        var pairs = new List<KeyValuePair<string, string>>();

        foreach (var token in text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = token.IndexOf('=');
            pairs.Add(separator < 0
                ? new KeyValuePair<string, string>(token, string.Empty)
                : new KeyValuePair<string, string>(token[..separator], token[(separator + 1)..]));
        }

        return pairs;
    }

    private static string Join(List<KeyValuePair<string, string>> pairs) =>
        pairs.Count == 0 ? string.Empty : string.Concat(pairs.Select(pair => $"{pair.Key}={pair.Value};"));
}
