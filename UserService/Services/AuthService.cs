using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.Transaction;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using UserService.Data;
using UserService.Entity;
using UserService.Helpers;
using UserService.Models;
using UserService.Models.Dtos;
using UserService.Repository;

namespace UserService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUserRepository repo, IMapper mapper, IOptions<JwtSettings> jwtSettings, IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _mapper = mapper;
            _jwtSettings = jwtSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var exists = await _repo.GetByEmailAsync(dto.Email);
            if (exists != null)
                throw new Exception("Email already in use");
            var accountNumExists = await _repo.GetByAccountNumberAsync(dto.AccountNumber);
            if (accountNumExists != null)
                throw new Exception("Account number is already used for another user");

            var user = _mapper.Map<User>(dto);

            using var hmac = new HMACSHA512();
            user.PasswordSalt = hmac.Key;
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
            user.Balance = 0;
            user.Role = Role.User;

            await _repo.AddUserAsync(user);
            await _repo.SaveChangesAsync();

            return await GenerateTokens(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Invalid credentials");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
            if (!hash.SequenceEqual(user.PasswordHash))
                throw new Exception("Invalid credentials");

            return await GenerateTokens(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token)
        {
            var user = await _repo.GetByRefreshTokenAsync(token);

            var refreshToken = user?.RefreshTokens.FirstOrDefault(r => r.Token == token);
            if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
                throw new Exception("Invalid or expired refresh token");

            refreshToken.IsRevoked = true;
            await _repo.SaveChangesAsync();

            return await GenerateTokens(user!);
        }


        private async Task<AuthResponseDto> GenerateTokens(User user)
        {
            var accessToken = TokenHelper.GenerateToken(user, _jwtSettings);
            var refreshToken = TokenHelper.GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                IsRevoked = false
            });

            await _repo.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        public async Task<List<User>> GetUsersByIdsAsync(List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return new List<User>();

            var users = await _repo.GetUsersByIdsAsync(userIds);
            return users;
        }
        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _repo.GetAllAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task UpdateUserRoleAsync(int id, Role newRole)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            user.Role = newRole;
            await _repo.SaveChangesAsync();
        }
        public async Task TransferAsync(TransferRequestDto dto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userIdFromToken))
                throw new UnauthorizedAccessException("Invalid user ID in token");

            var allAccountNumbers = dto.Transfers.Select(t => t.UserAccountNumber)
                                                 .Append(dto.MerchantAccountNumber)
                                                 .Distinct()
                                                 .ToList();

            var userMap = await _repo.GetUserIdsFromAccountNumbersAsync(allAccountNumbers);
            if (userMap.Count != allAccountNumbers.Count)
                throw new ArgumentException("Some account numbers not found");

            var merchantId = userMap.First(x => x.AccountNumber == dto.MerchantAccountNumber).UserId;

            if (userIdFromToken != merchantId)
                throw new UnauthorizedAccessException("Token does not match the merchant");

            var merchant = await _repo.GetByIdAsync(merchantId);
            if ((merchant == null || merchant.Role != Role.Merchant)&&(merchant == null || merchant.Role != Role.User))
                throw new ArgumentException("Merchant not found or invalid role");

            decimal totalTransferAmount = dto.Transfers.Sum(t => t.Amount);
            if (merchant.Balance < totalTransferAmount)
                throw new ArgumentException("Insufficient merchant balance");

            var recipientAccountNumbers = dto.Transfers.Select(t => t.UserAccountNumber).ToList();
            var recipientUserIds = userMap.Where(x => x.AccountNumber != dto.MerchantAccountNumber)
                                          .ToDictionary(x => x.AccountNumber, x => x.UserId);

            var recipients = await _repo.GetUsersByIdsAsync(recipientUserIds.Values.ToList());
            if (recipients.Count != recipientUserIds.Count)
                throw new ArgumentException("Some recipients not found");

            foreach (var transfer in dto.Transfers)
            {
                var recipientId = recipientUserIds[transfer.UserAccountNumber];
                var recipient = recipients.FirstOrDefault(u => u.Id == recipientId);
                if (recipient != null)
                {
                    recipient.Balance += transfer.Amount;
                }
            }

            merchant.Balance -= totalTransferAmount;

            await _repo.SaveChangesAsync();
        }


        public async Task<List<SharedLibrary.Transaction.AccountNumberToIdDto>> GetUserIdsFromAccountNumbersAsync(List<string> accountNumbers)
        {
            var result = await _repo.GetUserIdsFromAccountNumbersAsync(accountNumbers);
            return result.Select(r => new SharedLibrary.Transaction.AccountNumberToIdDto
            {
                AccountNumber = r.AccountNumber,
                UserId = r.UserId
            }).ToList();
        }


    }
}
