using Labverse.BLL.DTOs.Users;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Labverse.BLL.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFirebaseAuthService _firebaseAuthService;

    public UserService(IUnitOfWork unitOfWork, IFirebaseAuthService firebaseAuthService)
    {
        _unitOfWork = unitOfWork;
        _firebaseAuthService = firebaseAuthService;
    }

    public async Task<UserDto> AddAsync(CreateUserDto dto)
    {
        var existing = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("Email already exists");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = await _unitOfWork.Users.AddAsync(
            new User
            {
                Email = dto.Email,
                PasswordHash = passwordHash,
                Username = dto.Username,
                Level = 1,
                Points = 0,
            }
        );

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto?> Authenticate(AuthRequestDto dto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);

        if (user == null || !user.IsActive)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return MapToDto(user);
    }

    public async Task<UserDto> AuthenticateWithFirebaseAsync(ExternalAuthRequestDto dto)
    {
        var verified = await _firebaseAuthService.VerifyIdTokenAsync(dto.IdToken);

        var uid = verified.Uid;
        var email = verified.Claims.GetValueOrDefault("email")?.ToString();
        var name = verified.Claims.GetValueOrDefault("name")?.ToString();
        var picture = verified.Claims.GetValueOrDefault("picture")?.ToString();

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Firebase token missing email");

        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if (user == null)
        {
            user = new User
            {
                FirebaseUid = uid,
                AuthProvider = "google",
                Email = email!,
                EmailVerifiedAt = DateTime.UtcNow,
                Username = string.IsNullOrWhiteSpace(name) ? email! : name!,
                AvatarUrl = picture,
                PasswordHash = null!, // no password for Firebase users
            };
            user = await _unitOfWork.Users.AddAsync(user);
        }
        else
        {
            if (user.EmailVerifiedAt == null)
            {
                user.EmailVerifiedAt = DateTime.UtcNow;
            }

            user.FirebaseUid = uid;
            user.AuthProvider = "google";

            _unitOfWork.Users.Update(user);
        }

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found");
        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(int pointsAwarded, UserDto user, string? message)> ClaimDailyLoginAsync(
        int userId
    )
    {
        var user =
            await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        if (user.EmailVerifiedAt == null)
        {
            return (0, MapToDto(user), "Email not verified");
        }

        var today = DateTime.UtcNow.Date;
        var last = user.LastActiveAt?.Date;
        if (last != null && last >= today)
        {
            return (0, MapToDto(user), "Already claimed for today");
        }

        user.Points += Gamification.XpRules.DailyLoginXp;
        var (_, milestone) = Gamification.StreakHelper.UpdateForActivity(user, DateTime.UtcNow);
        var totalAwarded = Gamification.XpRules.DailyLoginXp + milestone;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return (totalAwarded, MapToDto(user), null);
    }

    public async Task RestoreAsync(int id)
    {
        var user = await _unitOfWork
            .Users.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            throw new KeyNotFoundException("User not found");
        if (user.IsActive)
            return; // already active
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(
        bool? isOnlyVerifiedUser = false,
        bool includeInactive = false
    )
    {
        var baseQuery = _unitOfWork.Users.Query().Include(u => u.UserSubscriptions);

        // includeInactive toggles off global IsActive filter
        if (includeInactive)
        {
            // Remove global filter by using IgnoreQueryFilters on DbSet
            baseQuery = _unitOfWork
                .Users.Query()
                .IgnoreQueryFilters()
                .Include(u => u.UserSubscriptions);
        }

        var users = await baseQuery.ToListAsync();

        if (isOnlyVerifiedUser == true)
        {
            users = users.Where(u => u.EmailVerifiedAt != null).ToList();
        }

        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _unitOfWork
            .Users.Query()
            .Include(u => u.UserSubscriptions)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user == null ? null : MapToDto(user);
    }

    public async Task UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        if (!string.IsNullOrWhiteSpace(dto.Username))
            user.Username = dto.Username;

        if (!string.IsNullOrWhiteSpace(dto.AvatarUrl))
            user.AvatarUrl = dto.AvatarUrl;

        if (!string.IsNullOrWhiteSpace(dto.Bio))
            user.Bio = dto.Bio;

        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int id, ChangePasswordUserDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid old password");

        if (dto.OldPassword.Equals(dto.NewPassword))
            throw new InvalidOperationException("New password must be different from old password");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private static UserDto MapToDto(User user)
    {
        var now = DateTime.UtcNow;
        var isUserScriptionActive = user.UserSubscriptions.Any(us =>
            us.StartDate <= now && us.EndDate > now
        );

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsActive = user.IsActive,
            EmailVerifiedAt = user.EmailVerifiedAt,
            Subscription = isUserScriptionActive ? "Premium" : "Free",
            Points = user.Points,
            Level = user.Level,
            StreakCurrent = user.StreakCurrent,
            StreakBest = user.StreakBest,
            LastActiveAt = user.LastActiveAt,
        };
    }
}
