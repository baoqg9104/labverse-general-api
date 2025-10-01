using Labverse.DAL.EntitiesModels;
using Microsoft.EntityFrameworkCore;

namespace Labverse.DAL.Data;

public class LabverseDbContext : DbContext
{
    public LabverseDbContext(DbContextOptions<LabverseDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<Lab> Labs { get; set; }
    public DbSet<LabCategory> LabCategories { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<UserProgress> UserProgresses { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasQueryFilter(u => u.IsActive);
        modelBuilder.Entity<Lab>().HasQueryFilter(u => u.IsActive);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProgress>()
            .HasOne(up => up.User)
            .WithMany(u => u.Progresses)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserProgress>()
            .HasOne(up => up.Lab)
            .WithMany()
            .HasForeignKey(up => up.LabId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lab>()
        .HasIndex(l => l.Slug)
        .IsUnique();
    }
}
