using System.Text.Json.Serialization;

namespace HeroRegistry.Models;

[JsonConverter(typeof(JsonStringEnumConverter<HeroRace>))]
public enum HeroRace
{
    Human = 1,
    Alien = 2,
    Mutant = 3,
    Asgardian = 4
}
