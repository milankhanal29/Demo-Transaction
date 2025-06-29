using Microsoft.EntityFrameworkCore;
using UserService.Entity;

namespace UserService.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByAccountNumberAsync(string accountNumber);
        Task<User?> GetByIdAsync(int id);
        Task AddUserAsync(User user);
        Task SaveChangesAsync();
        Task<User?> GetByRefreshTokenAsync(string token);
        Task<List<User>> GetAllAsync();
        Task<List<User>> GetUsersByIdsAsync(List<int> userIds);
        Task<List<(string AccountNumber, int UserId)>> GetUserIdsFromAccountNumbersAsync(List<string> accountNumbers);

    }
}
