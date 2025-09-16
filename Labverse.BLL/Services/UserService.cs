using Labverse.BLL.DTOs.Users;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;

namespace Labverse.BLL.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> AddAsync(CreateUserDto dto)
    {
        var existing = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("Email already exists");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = await _unitOfWork.Users.AddAsync(new User
        {
            Email = dto.Email,
            PasswordHash = passwordHash,
            Username = dto.Username,
        });

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

    public async Task DeleteAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.PasswordHash = passwordHash;
        }

        user.Username = dto.Username;
        user.AvatarUrl = dto.AvatarUrl;
        user.Bio = dto.Bio;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private static UserDto MapToDto(User user)
    {
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
            IsActive = user.IsActive
        };
    }
}
