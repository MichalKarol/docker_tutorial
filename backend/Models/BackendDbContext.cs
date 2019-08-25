using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace backend.Models {
    public class BackendDbContext: DbContext {
        public BackendDbContext(DbContextOptions<BackendDbContext> contextOptions): base(contextOptions) {
        }
        public DbSet<Settings> Settings { get; set; }

        public DbSet<Worker> Workers { get; set; }

        public DbSet<LoadHistory> LoadHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Settings>().HasData(
                new Settings(){Id = -1, Generated = 100, Consumed = 100}
            );
            modelBuilder.Entity<Worker>().HasMany(w => w.LoadHistory).WithOne(lh => lh.Worker).OnDelete(DeleteBehavior.Cascade);
        }
    }
}