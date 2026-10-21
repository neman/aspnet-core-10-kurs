// TODO Task 2: Delete this file.

namespace HeroRegistry.Services;

public sealed class InMemoryDatabase
{
    public object Sync { get; } = new();
    public List<Power> Powers { get; } = new();
    public List<Hero> Heroes { get; } = new();

    public InMemoryDatabase()
    {
        // "(memory)" on a page means the data came from here; it disappears once EF Core is the store.
        var shield = new Power("Shield (memory)") { Id = 1 };
        var light = new Power("Light (memory)") { Id = 2 };
        var flight = new Power("Flight (memory)") { Id = 3 };
        Powers.AddRange([shield, light, flight]);

        var sentinel = new Hero(
            "Sentinel",
            "A human engineer dedicated to protecting the city.",
            HeroRace.Human,
            HeroAlignment.Heroic) { Id = 1 };
        sentinel.AddPower(shield);
        sentinel.Register();

        var aurora = new Hero(
            "Aurora",
            "An alien explorer who defends her adopted home.",
            HeroRace.Alien,
            HeroAlignment.Heroic) { Id = 2 };
        aurora.AddPower(light);
        aurora.AddPower(flight);
        aurora.Register();

        var shadowling = new Hero(
            "Shadowling",
            "An apprentice still working on their origin story.",
            HeroRace.Mutant,
            HeroAlignment.Neutral) { Id = 3 };

        Heroes.AddRange([sentinel, aurora, shadowling]);
    }
}
