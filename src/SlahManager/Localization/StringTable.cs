using System.Text.Json;
using SlahManager.Infrastructure;

namespace SlahManager.Localization;

public static class StringTable
{
    public static Dictionary<string, string> Load(AppLanguage language)
    {
        try
        {
            var content = EmbeddedData.ReadText($"Strings/{language.Code()}.json");
            if (content is not null)
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(content) ?? new();
            }

            Logger.Error($"String table not found: {language.Code()}");
        }
        catch (Exception exception)
        {
            Logger.Error($"Could not read string table {language.Code()}", exception);
        }

        return new();
    }
}
