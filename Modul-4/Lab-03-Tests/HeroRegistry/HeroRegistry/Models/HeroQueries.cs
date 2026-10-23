namespace HeroRegistry.Models;

public static class HeroQueries
{
    public static IQueryable<Hero> Registered(this IQueryable<Hero> heroes) =>
        heroes.Where(h => h.Status == HeroStatus.Registered)
            .OrderBy(h => h.Codename);

    public static IQueryable<Hero> Drafts(this IQueryable<Hero> heroes) =>
        heroes.Where(h => h.Status == HeroStatus.Draft)
            .OrderBy(h => h.Codename);

    public static IQueryable<Hero> OwnedBy(this IQueryable<Hero> heroes, string ownerName) =>
        heroes.Where(h => h.OwnerName == ownerName);
}
