namespace HeroRegistry.ViewModels;

public sealed record HeroDetailsViewModel(
    int Id,
    string Codename,
    string OriginStory,
    string Race,
    string Alignment,
    IReadOnlyList<string> Powers);
