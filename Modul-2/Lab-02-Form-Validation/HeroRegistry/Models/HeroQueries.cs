namespace HeroRegistry.Models;

// Query rules in one place; they work on any IQueryable<Hero>, in memory or in EF Core.
public static class HeroQueries
{
    // Public registry: only Registered heroes, sorted by Codename.
    public static IQueryable<Hero> Registered(this IQueryable<Hero> heroes) =>
        heroes.Where(h => h.Status == HeroStatus.Registered)
            .OrderBy(h => h.Codename);

    public static IQueryable<Hero> Drafts(this IQueryable<Hero> heroes) =>
        heroes.Where(h => h.Status == HeroStatus.Draft)
            .OrderBy(h => h.Codename);
}
