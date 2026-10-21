using HeroRegistry.Models;
using Microsoft.EntityFrameworkCore;

namespace HeroRegistry.Data;

// Runs through UseSeeding: after Update-Database and after Database.Migrate().
public static class HeroSeed
{
    public static void Seed(DbContext db)
    {
        // TODO Task 3: Replace this body: seed Sentinel, Aurora and Shadowling (code in README).
        if (db.Set<Power>().Any())
            return;

        db.Set<Power>().AddRange(new Power("Shield"), new Power("Light"), new Power("Flight"));
        db.SaveChanges();
    }
}
