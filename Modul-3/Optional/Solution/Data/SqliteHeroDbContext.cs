namespace HeroRegistry.Data;

// Optional C: the same model on SQLite. A separate type gets separate migrations
// (Migrations/Sqlite), because SQL generated for SQL Server does not run on SQLite.
public sealed class SqliteHeroDbContext(DbContextOptions<SqliteHeroDbContext> options)
    : HeroDbContext(options);
