using System.Net;

namespace HeroRegistry.Tests;

public sealed class HeroesPagesTests(HeroRegistryFactory factory) : IClassFixture<HeroRegistryFactory>
{
    [Fact]
    public async Task Anonymous_browser_is_sent_to_the_login_page()
    {
        var client = factory.CreateClientAs(null);

        var response = await client.GetAsync("/Heroes/Create");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("http://localhost/Account/Login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Signed_in_user_sees_only_their_own_drafts()
    {
        var bob = factory.CreateClientAs("bob");

        var html = await bob.GetStringAsync("/Heroes/Drafts");

        Assert.Contains("No drafts yet", html);
        Assert.DoesNotContain("Shadowling", html); // alice's seed Draft
    }
}
