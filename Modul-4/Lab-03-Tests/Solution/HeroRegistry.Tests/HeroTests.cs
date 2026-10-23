using HeroRegistry.Models;

namespace HeroRegistry.Tests;

// Names describe behaviour a domain expert would recognise, not the method under test.
public sealed class HeroTests
{
    [Fact]
    public void Draft_without_powers_cannot_be_registered()
    {
        var hero = NewDraft("A night guard who protects the old harbor district.");

        var error = Assert.Throws<BusinessRuleException>(() => hero.Register());

        Assert.Equal("A hero needs at least one power.", error.Message);
        Assert.Equal(HeroStatus.Draft, hero.Status);
    }

    [Fact]
    public void Origin_story_with_a_forbidden_phrase_blocks_registration()
    {
        var hero = NewDraft("The Chosen One of an ancient order of guardians.");
        hero.AddPower(new Power("Light"));

        var error = Assert.Throws<BusinessRuleException>(() => hero.Register());

        Assert.Contains("chosen one", error.Message);
        Assert.Equal(HeroStatus.Draft, hero.Status);
    }

    [Fact]
    public void Draft_with_a_power_and_a_long_enough_story_becomes_registered()
    {
        var hero = NewDraft("A night guard who protects the old harbor district.");
        hero.AddPower(new Power("Shield"));

        hero.Register();

        Assert.Equal(HeroStatus.Registered, hero.Status);
    }

    [Fact]
    public void Only_a_registered_hero_can_retire()
    {
        var hero = NewDraft("A night guard who protects the old harbor district.");

        var error = Assert.Throws<InvalidStatusException>(() => hero.Retire());

        Assert.Equal(HeroStatus.Draft, error.Status);
    }

    [Fact]
    public void Retired_hero_is_read_only()
    {
        var hero = NewDraft("A night guard who protects the old harbor district.");
        hero.AddPower(new Power("Shield"));
        hero.Register();
        hero.Retire();

        Assert.Throws<InvalidStatusException>(() => hero.AddPower(new Power("Flight")));
    }

    [Fact]
    public void Hero_without_an_owner_cannot_be_created()
    {
        Assert.Throws<BusinessRuleException>(() => new Hero(
            "",
            "Nightwatch",
            "A night guard who protects the old harbor district.",
            HeroRace.Human,
            HeroAlignment.Heroic));
    }

    private static Hero NewDraft(string originStory) => new(
        "alice",
        "Nightwatch",
        originStory,
        HeroRace.Human,
        HeroAlignment.Heroic);
}
