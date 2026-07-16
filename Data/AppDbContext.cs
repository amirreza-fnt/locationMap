using Microsoft.EntityFrameworkCore;
using LocationMap.API.Models;

namespace LocationMap.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MapPoint> MapPoints => Set<MapPoint>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MapPointMedia> MapPointMedia => Set<MapPointMedia>();
    public DbSet<MapUser> MapUsers => Set<MapUser>();
    public DbSet<Guide> Guides => Set<Guide>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<MapPoint>(entity =>
        {
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.GuideId);
            entity.HasIndex(e => e.SubmittedById);
            entity.HasIndex(e => e.Latitude);
            entity.HasIndex(e => e.Longitude);

            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Latitude).HasPrecision(10, 8);
            entity.Property(e => e.Longitude).HasPrecision(11, 8);

            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Guide)
                  .WithMany(g => g.MapPoints)
                  .HasForeignKey(e => e.GuideId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SubmittedBy)
                  .WithMany(u => u.SubmittedPoints)
                  .HasForeignKey(e => e.SubmittedById)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReviewedBy)
                  .WithMany(u => u.ReviewedPoints)
                  .HasForeignKey(e => e.ReviewedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.SortOrder);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Guide>(entity =>
        {
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(100);

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Guides)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MapPointMedia>(entity =>
        {
            entity.HasIndex(e => e.MapPointId);
            entity.HasOne(e => e.MapPoint)
                  .WithMany(m => m.Media)
                  .HasForeignKey(e => e.MapPointId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MapUser>(entity =>
        {
            entity.HasIndex(e => e.MelliCode);
            entity.HasIndex(e => e.Phone);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is MapPoint &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                ((MapPoint)entry.Entity).UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
