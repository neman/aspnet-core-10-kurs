using System.Text.Json.Serialization;

namespace HeroRegistry.Models;

[JsonConverter(typeof(JsonStringEnumConverter<HeroStatus>))]
public enum HeroStatus
{
    Draft,
    Registered,
    Retired
}
