using System.Text.Json.Serialization;

namespace HeroRegistry.Models;

[JsonConverter(typeof(JsonStringEnumConverter<HeroAlignment>))]
public enum HeroAlignment
{
    Heroic = 1,
    Neutral = 2,
    Villainous = 3
}
