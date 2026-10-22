using HeroRegistry.Models;
using Microsoft.EntityFrameworkCore;

namespace HeroRegistry.Data;

// Runs through UseSeeding: after Update-Database and after Database.Migrate().
public static class HeroSeed
{
    public static void Seed(DbContext db)
    {
        if (db.Set<Hero>().Any())
            return;

        var sentinel = new Hero(
            "Sentinel",
            "A human engineer dedicated to protecting the city.",
            HeroRace.Human,
            HeroAlignment.Heroic);
        sentinel.AddPower(GetPower(db, "Shield"));
        sentinel.Register();

        var aurora = new Hero(
            "Aurora",
            "An alien explorer who defends her adopted home.",
            HeroRace.Alien,
            HeroAlignment.Heroic);
        aurora.AddPower(GetPower(db, "Light"));
        aurora.AddPower(GetPower(db, "Flight"));
        aurora.Register();

        var shadowling = new Hero(
            "Shadowling",
            "An apprentice still working on their origin story.",
            HeroRace.Mutant,
            HeroAlignment.Neutral);

        db.Set<Hero>().AddRange(sentinel, aurora, shadowling);
        db.SaveChanges();
    }

    // Reuses a power that is already in the database; a new one is saved with its hero.
    private static Power GetPower(DbContext db, string name) =>
        db.Set<Power>().FirstOrDefault(p => p.Name == name) ?? new Power(name);
}
