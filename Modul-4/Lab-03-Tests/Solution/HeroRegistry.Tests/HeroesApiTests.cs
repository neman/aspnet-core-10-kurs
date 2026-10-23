using System.Net;
using System.Net.Http.Json;
using HeroRegistry.Api;
using HeroRegistry.Models;

namespace HeroRegistry.Tests;

// One factory (and one database) per test class; each test creates the heroes it needs.
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

    [Fact]
    public async Task Anonymous_caller_cannot_create_a_hero()
    {
        var client = factory.CreateClientAs(null);

        var response = await client.PostAsJsonAsync("/api/heroes", NewHero("Nobody"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.Single().Scheme);
    }

    [Fact]
    public async Task Created_hero_belongs_to_the_caller()
    {
        var client = factory.CreateClientAs("bob");

        var response = await client.PostAsJsonAsync("/api/heroes", NewHero("Skylark"));
        var hero = await response.Content.ReadFromJsonAsync<HeroResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("bob", hero!.Owner);
        Assert.Equal(HeroStatus.Draft, hero.Status);
    }

    [Fact]
    public async Task Bob_cannot_delete_Alices_draft()
    {
        var alice = factory.CreateClientAs("alice");
        var bob = factory.CreateClientAs("bob");
        var draft = await CreateDraftAsync(alice, "Nightwatch");

        var response = await bob.DeleteAsync($"/api/heroes/{draft.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await alice.GetAsync($"/api/heroes/{draft.Id}")).StatusCode);
    }

    [Fact]
    public async Task Owner_can_delete_own_draft()
    {
        var alice = factory.CreateClientAs("alice");
        var draft = await CreateDraftAsync(alice, "Shortlived");

        var response = await alice.DeleteAsync($"/api/heroes/{draft.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await alice.GetAsync($"/api/heroes/{draft.Id}")).StatusCode);
    }

    [Fact]
    public async Task Registered_hero_cannot_be_deleted_even_by_its_owner()
    {
        var alice = factory.CreateClientAs("alice");

        // Aurora (id 2) is a seed hero owned by alice and already Registered.
        var response = await alice.DeleteAsync("/api/heroes/2");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Contains("Only a Draft hero can be deleted", problem!.Detail);
    }

    [Fact]
    public async Task Only_an_admin_can_retire_a_hero()
    {
        var bob = factory.CreateClientAs("bob");

        var response = await bob.PostAsync("/api/heroes/1/retire", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_retire_a_registered_hero()
    {
        var admin = factory.CreateClientAs("alice", "Admin");
        var draft = await CreateDraftAsync(admin, "Veteran");
        await admin.PostAsync($"/api/heroes/{draft.Id}/powers/1", null);
        await admin.PostAsync($"/api/heroes/{draft.Id}/register", null);

        var response = await admin.PostAsync($"/api/heroes/{draft.Id}/retire", null);
        var hero = await response.Content.ReadFromJsonAsync<HeroResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HeroStatus.Retired, hero!.Status);
    }

    private static CreateHeroRequest NewHero(string codename) => new()
    {
        Codename = codename,
        OriginStory = "A night guard who protects the old harbor district.",
        Race = HeroRace.Human,
        Alignment = HeroAlignment.Heroic
    };

    private static async Task<HeroResponse> CreateDraftAsync(HttpClient client, string codename)
    {
        var response = await client.PostAsJsonAsync("/api/heroes", NewHero(codename));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<HeroResponse>())!;
    }

    private sealed record ProblemDetails(string? Title, int? Status, string? Detail);
}
