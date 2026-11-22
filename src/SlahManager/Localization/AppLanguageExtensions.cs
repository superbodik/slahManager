namespace SlahManager.Localization;

public static class AppLanguageExtensions
{
    public static string Code(this AppLanguage language) => language switch
    {
        AppLanguage.Russian => "ru",
        _ => "en"
    };
}
