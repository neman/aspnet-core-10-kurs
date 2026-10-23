using HeroRegistry.Models;

namespace HeroRegistry.Tests;

// Names describe behaviour a domain expert would recognise, not the method under test.
public sealed class HeroTests
{
    [Fact]
    public void Draft_without_powers_cannot_be_registered()
    {
        var hero = new Hero(
            "alice",
            "Nightwatch",
            "A night guard who protects the old harbor district.",
            HeroRace.Human,
            HeroAlignment.Heroic);

        var error = Assert.Throws<BusinessRuleException>(() => hero.Register());

        Assert.Equal("A hero needs at least one power.", error.Message);
        Assert.Equal(HeroStatus.Draft, hero.Status);
    }
}
