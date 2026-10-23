using System.Net;
using System.Net.Http.Json;
using HeroRegistry.Api;

namespace HeroRegistry.Tests;

// One factory (and one database) per test class; each test leaves the seed data as it found it.
public sealed class HeroesApiTests(HeroRegistryFactory factory) : IClassFixture<HeroRegistryFactory>
{
    [Fact]
    public async Task Public_list_shows_registered_heroes_without_a_token()
    {
        var client = factory.CreateClientAs(null);

        var heroes = await client.GetFromJsonAsync<List<HeroResponse>>("/api/heroes");

        Assert.NotNull(heroes);
        Assert.Equal(["Aurora", "Sentinel"], heroes.Select(h => h.Codename));
    }
}
