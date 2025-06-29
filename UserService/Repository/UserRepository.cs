using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Entity;

namespace UserService.Repository
{
    public class UserRepository:IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.AccountNumber == accountNumber);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<User?> GetByRefreshTokenAsync(string token)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens.Any(r => r.Token == token && !r.IsRevoked));
        }
        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<List<User>> GetUsersByIdsAsync(List<int> userIds)
        {
            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }
        public async Task<List<(string AccountNumber, int UserId)>> GetUserIdsFromAccountNumbersAsync(List<string> accountNumbers)
        {
            return await _context.Users
                .Where(u => accountNumbers.Contains(u.AccountNumber))
                .Select(u => new ValueTuple<string, int>(u.AccountNumber, u.Id))
                .ToListAsync();
        }


    }
}
