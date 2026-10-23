using System.ComponentModel.DataAnnotations;
using HeroRegistry.Models;

namespace HeroRegistry.Api;

// Properties, not a positional record: the controller and the Minimal API
// ValidationFilter both read the attributes from properties.
public sealed record CreateHeroRequest
{
    [Required, StringLength(60)] public string Codename { get; init; } = "";
    [StringLength(1000)] public string? OriginStory { get; init; }
    [Required, EnumDataType(typeof(HeroRace))] public HeroRace? Race { get; init; }
    [Required, EnumDataType(typeof(HeroAlignment))] public HeroAlignment? Alignment { get; init; }
}

public sealed record HeroResponse(
    int Id,
    string Owner,
    string Codename,
    string OriginStory,
    HeroRace Race,
    HeroAlignment Alignment,
    HeroStatus Status,
    IReadOnlyList<string> Powers)
{
    public static HeroResponse From(Hero hero) => new(
        hero.Id, hero.OwnerName, hero.Codename, hero.OriginStory, hero.Race, hero.Alignment, hero.Status,
        hero.Powers.Select(p => p.Name).ToList());
}
