using HeroRegistry.Models;

namespace HeroRegistry.Services;

// Shaped like EF Core's DbContext: query through Heroes and Powers,
// stage changes with Add/Remove and write them with SaveChanges.
public interface IHeroStore
{
    IQueryable<Hero> Heroes { get; }
    IQueryable<Power> Powers { get; }

    void Add(Hero hero);
    void Add(Power power);
    void Remove(Hero hero);

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
