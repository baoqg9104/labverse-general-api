using Labverse.BLL.DTOs.Users;

namespace Labverse.BLL.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto?> Authenticate(AuthRequestDto dto);
    Task<IEnumerable<UserDto>> GetAllAsync(bool? isOnlyVerifiedUser = false);
    Task<UserDto> AddAsync(CreateUserDto dto);
    Task UpdateAsync(int id, UpdateUserDto dto);
    Task ChangePasswordAsync(int id, ChangePasswordUserDto dto);
    Task DeleteAsync(int id);
}
