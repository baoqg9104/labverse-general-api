using Labverse.DAL.EntitiesModels;
using Microsoft.EntityFrameworkCore;

namespace Labverse.DAL.Data;

public class LabverseDbContext : DbContext
{
    public LabverseDbContext(DbContextOptions<LabverseDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<Lab> Labs { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<UserProgress> UserProgresses { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<LabQuestion> LabQuestions { get; set; }
    public DbSet<UserLabAnswer> UserLabAnswers { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<ActivityHistory> ActivityHistories { get; set; }
    public DbSet<LabView> LabViews { get; set; }
    public DbSet<LabRating> LabRatings { get; set; }
    public DbSet<LabComment> LabComments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasQueryFilter(u => u.IsActive);
        modelBuilder.Entity<Lab>().HasQueryFilter(u => u.IsActive);
        modelBuilder.Entity<ChatRoom>().HasQueryFilter(u => u.IsActive);
        modelBuilder.Entity<Message>().HasQueryFilter(u => u.IsActive);
        modelBuilder.Entity<Resource>().HasQueryFilter(u => u.IsActive);
        modelBuilder.Entity<LabQuestion>().HasQueryFilter(u => u.IsActive);
        modelBuilder.Entity<ActivityHistory>().HasQueryFilter(u => u.IsActive);
        modelBuilder.Entity<LabComment>().HasQueryFilter(u => u.IsActive);

        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<UserProgress>()
            .HasOne(up => up.User)
            .WithMany(u => u.Progresses)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<UserProgress>()
            .HasOne(up => up.Lab)
            .WithMany()
            .HasForeignKey(up => up.LabId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChatRoomUser>().HasKey(cru => new { cru.Id, cru.UserId });

        modelBuilder
            .Entity<ChatRoomUser>()
            .HasOne(cru => cru.ChatRoom)
            .WithMany(cr => cr.ChatRoomUsers)
            .HasForeignKey(cru => cru.Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<ChatRoomUser>()
            .HasOne(cru => cru.User)
            .WithMany(u => u.ChatRooms)
            .HasForeignKey(cru => cru.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lab>().HasIndex(l => l.Slug).IsUnique();

        // Lab questions
        modelBuilder
            .Entity<LabQuestion>()
            .HasOne(q => q.Lab)
            .WithMany(l => l.Questions)
            .HasForeignKey(q => q.LabId)
            .OnDelete(DeleteBehavior.Cascade);

        // User answers
        modelBuilder
            .Entity<UserLabAnswer>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<UserLabAnswer>()
            .HasOne(a => a.Lab)
            .WithMany()
            .HasForeignKey(a => a.LabId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<UserLabAnswer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Reports
        modelBuilder
            .Entity<Report>()
            .HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Report>()
            .HasOne(r => r.AssignedAdmin)
            .WithMany()
            .HasForeignKey(r => r.AssignedAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Report>().HasIndex(r => r.CreatedAt);
        modelBuilder.Entity<Report>().HasIndex(r => r.Status);
        modelBuilder.Entity<Report>().HasIndex(r => r.Type);
        modelBuilder.Entity<Report>().HasIndex(r => r.Severity);

        // ActivityHistory relations and indexes
        modelBuilder
            .Entity<ActivityHistory>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder
            .Entity<ActivityHistory>()
            .HasOne(a => a.Lab)
            .WithMany()
            .HasForeignKey(a => a.LabId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder
            .Entity<ActivityHistory>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<ActivityHistory>().HasIndex(a => a.UserId);
        modelBuilder.Entity<ActivityHistory>().HasIndex(a => a.CreatedAt);

        // Lab views
        modelBuilder
            .Entity<LabView>()
            .HasOne(v => v.Lab)
            .WithMany()
            .HasForeignKey(v => v.LabId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder
            .Entity<LabView>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LabView>().HasIndex(v => v.LabId);
        modelBuilder.Entity<LabView>().HasIndex(v => v.UserId);

        // Lab ratings
        modelBuilder
            .Entity<LabRating>()
            .HasOne(r => r.Lab)
            .WithMany()
            .HasForeignKey(r => r.LabId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder
            .Entity<LabRating>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LabRating>().HasIndex(r => new { r.LabId, r.UserId }).IsUnique();

        // Lab comments
        modelBuilder
            .Entity<LabComment>()
            .HasOne(c => c.Lab)
            .WithMany()
            .HasForeignKey(c => c.LabId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder
            .Entity<LabComment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LabComment>().HasIndex(c => c.LabId);
        modelBuilder.Entity<LabComment>().HasIndex(c => c.UserId);
        modelBuilder
            .Entity<LabComment>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
