using SharedLibrary.Transaction;
using UserService.Entity;
using UserService.Models;
using UserService.Models.Dtos;

namespace UserService.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task<UserDto?> GetByIdAsync(int id);
        Task<List<UserDto>> GetAllAsync();
        Task UpdateUserRoleAsync(int id, Role newRole);
        Task TransferAsync(TransferRequestDto dto);
        Task<List<User>> GetUsersByIdsAsync(List<int> userIds);
        Task<List<SharedLibrary.Transaction.AccountNumberToIdDto>> GetUserIdsFromAccountNumbersAsync(List<string> accountNumbers);
    }
}
