using Labverse.DAL.EntitiesModels;
using Labverse.DAL.Repositories.Interfaces;

namespace Labverse.DAL.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IEmailVerificationTokenRepository EmailVerificationTokens { get; }

    IRepository<Badge> Badges { get; }
    IRepository<Subscription> Subscriptions { get; }
    IRepository<UserSubscription> UserSubscriptions { get; }
    IRepository<UserProgress> UserProgresses { get; }
    IRepository<Lab> Labs { get; }
    IRepository<UserBadge> UserBadges { get; }
    IRepository<LabQuestion> LabQuestions { get; }
    IRepository<UserLabAnswer> UserLabAnswers { get; }
    IRepository<Report> Reports { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}