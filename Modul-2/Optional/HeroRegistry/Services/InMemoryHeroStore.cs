using HeroRegistry.Models;

namespace HeroRegistry.Services;

// Unit of work for one request (Scoped), like DbContext.
// Add/Remove are only staged; SaveChanges writes them to the shared database.
// Teaching simplification: queried heroes are the same objects as in the database,
// so changes made through their methods are visible before SaveChanges.
public sealed class InMemoryHeroStore(InMemoryDatabase db) : IHeroStore
{
    private readonly List<Hero> _addedHeroes = new();
    private readonly List<Power> _addedPowers = new();
    private readonly List<Hero> _removedHeroes = new();

    // A snapshot, so a query never runs while another request changes the list.
    public IQueryable<Hero> Heroes
    {
        get { lock (db.Sync) return db.Heroes.ToList().AsQueryable(); }
    }

    public IQueryable<Power> Powers
    {
        get { lock (db.Sync) return db.Powers.ToList().AsQueryable(); }
    }

    public void Add(Hero hero) => _addedHeroes.Add(hero);
    public void Add(Power power) => _addedPowers.Add(power);
    public void Remove(Hero hero) => _removedHeroes.Add(hero);

    public int SaveChanges()
    {
        lock (db.Sync)
        {
            foreach (var hero in _addedHeroes)
            {
                hero.Id = db.Heroes.Count == 0 ? 1 : db.Heroes.Max(h => h.Id) + 1;
                db.Heroes.Add(hero);
            }

            foreach (var power in _addedPowers)
            {
                power.Id = db.Powers.Count == 0 ? 1 : db.Powers.Max(p => p.Id) + 1;
                db.Powers.Add(power);
            }

            foreach (var hero in _removedHeroes)
                db.Heroes.Remove(hero);

            int count = _addedHeroes.Count + _addedPowers.Count + _removedHeroes.Count;
            _addedHeroes.Clear();
            _addedPowers.Clear();
            _removedHeroes.Clear();
            return count;
        }
    }
}
