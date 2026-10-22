using HeroRegistry.Services;

namespace HeroRegistry.Data;

public sealed class HeroDbContext(DbContextOptions<HeroDbContext> options)
    : DbContext(options), IHeroStore
{
    // No lazy loading in EF Core, and Register() needs the powers.
    public IQueryable<Hero> Heroes => Set<Hero>().Include(h => h.Powers);
    public IQueryable<Power> Powers => Set<Power>();

    public void Add(Hero hero) => Set<Hero>().Add(hero);
    public void Add(Power power) => Set<Power>().Add(power);
    public void Remove(Hero hero) => Set<Hero>().Remove(hero);

    // SaveChanges() comes from DbContext and implements IHeroStore.SaveChanges().

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Power>(power =>
        {
            power.ToTable("Powers");
            power.Property(p => p.Name).HasMaxLength(40);
        });

        modelBuilder.Entity<Hero>(hero =>
        {
            hero.ToTable("Heroes");
            hero.Property(h => h.Codename).HasMaxLength(60);
            hero.Property(h => h.OriginStory).HasMaxLength(1000);

            hero.Property(h => h.Race).HasConversion<string>().HasMaxLength(20);
            hero.Property(h => h.Alignment).HasConversion<string>().HasMaxLength(20);
            hero.Property(h => h.Status).HasConversion<string>().HasMaxLength(20);

            // Hero.Powers is read-only; EF Core fills the private _powers field.
            hero.HasMany(h => h.Powers)
                .WithMany()
                .UsingEntity(join => join.ToTable("HeroPowers"));
        });
    }
}
