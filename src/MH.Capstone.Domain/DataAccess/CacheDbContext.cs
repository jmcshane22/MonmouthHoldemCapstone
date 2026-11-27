using MH.Capstone.Domain.DataModels;
using Microsoft.EntityFrameworkCore;

namespace MH.Capstone.Domain.DataAccess
{
    public class CacheDbContext : DbContext
    {
        public CacheDbContext(DbContextOptions<CacheDbContext> options) : base(options)
        {
        }

        public DbSet<NinjaAnimalCacheEntity> AnimalApiCache { get; set; } = null!;

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
            => configurationBuilder.UseUtcDateTimeOffsetsOnPostgres(this);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configuration for cache entity
            modelBuilder.ApplyConfiguration(new NinjaAnimalCacheEntityConfiguration());
        }
    }
}
