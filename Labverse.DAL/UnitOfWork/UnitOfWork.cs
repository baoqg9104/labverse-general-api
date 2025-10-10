using Labverse.DAL.Data;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.Repositories;
using Labverse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Labverse.DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly LabverseDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public IUserRepository Users { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IEmailVerificationTokenRepository EmailVerificationTokens { get; }

    public IRepository<Badge> Badges { get; }
    public IRepository<Subscription> Subscriptions { get; }
    public IRepository<UserSubscription> UserSubscriptions { get; }
    public IRepository<UserProgress> UserProgresses { get; }
    public IRepository<Lab> Labs { get; }
    public IRepository<UserBadge> UserBadges { get; }
    public IRepository<LabQuestion> LabQuestions { get; }
    public IRepository<UserLabAnswer> UserLabAnswers { get; }

    public UnitOfWork(
        LabverseDbContext context,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IEmailVerificationTokenRepository emailVerificationTokenRepository
    )
    {
        _context = context;

        Users = userRepository;
        RefreshTokens = refreshTokenRepository;
        EmailVerificationTokens = emailVerificationTokenRepository;

        Badges = new Repository<Badge>(_context);
        Subscriptions = new Repository<Subscription>(_context);
        UserSubscriptions = new Repository<UserSubscription>(_context);
        UserProgresses = new Repository<UserProgress>(_context);
        Labs = new Repository<Lab>(_context);
        UserBadges = new Repository<UserBadge>(_context);
        LabQuestions = new Repository<LabQuestion>(_context);
        UserLabAnswers = new Repository<UserLabAnswer>(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
            return;
        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
