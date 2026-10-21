namespace HeroRegistry.Data;

// Demo 1: only Power is mapped so far. In Lab 1 the context also maps Hero
// and becomes the application's IHeroStore.
// TODO Task 2: Also implement IHeroStore: Heroes, Add(Hero), Add(Power), Remove(Hero).
public sealed class HeroDbContext(DbContextOptions<HeroDbContext> options) : DbContext(options)
{
    public IQueryable<Power> Powers => Set<Power>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Power>(power =>
        {
            power.ToTable("Powers");
            power.Property(p => p.Name).HasMaxLength(40);
        });

        // TODO Task 1: Map Hero: table Heroes, max lengths, enums as text, powers through HeroPowers.
    }
}
