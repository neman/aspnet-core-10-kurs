namespace HeroRegistry.Services;

// Optional A: the "RegistrationRules" section of appsettings.json.
public sealed class RegistrationRulesOptions
{
    public const string SectionName = "RegistrationRules";

    public string[] ForbiddenPhrases { get; set; } = [];
}
