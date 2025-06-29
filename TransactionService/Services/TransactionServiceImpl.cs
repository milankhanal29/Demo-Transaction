using AutoMapper;
using OfficeOpenXml;
using SharedLibrary;
using System.Net.Http.Headers;
using System.Security.Claims;
using TransactionService.Models;
using TransactionService.Repository;
using TransactionService.Utils;
using TransferUserDto = TransactionService.Models.TransferUserDto;

namespace TransactionService.Services
{

    public class TransactionServiceImpl : ITransactionService
    {
        private readonly ITransactionRepository _repo;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionServiceImpl(ITransactionRepository repo,
                                  IHttpClientFactory httpFactory,
                                  IHttpContextAccessor httpContextAccessor,
                                  IMapper mapper)
        {
            _repo = repo;
            _httpFactory = httpFactory;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;

        }
        
            public async Task<List<TransactionDto>> GetAllTransactionsAsync()
        {

            var txs = await _repo.GetALlTransactionAsync();
            return _mapper.Map<List<TransactionDto>>(txs);
        }
        public async Task<PagedResult<OverallTransactionDto>> GetTransactionsForUserAsync(int userId, PaginationParams paginationParams)
        {
            var httpContext = _httpContextAccessor.HttpContext!;
            var userIdClaim = httpContext.User?.FindFirst("userId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int tokenUserId))
                throw new UnauthorizedAccessException("Invalid or missing token");

            if (tokenUserId != userId)
                throw new UnauthorizedAccessException("You can only view your own transactions");

            return await _repo.GetTransactionsWithNamesAsync(userId, paginationParams);
        }
        public async Task TransferAsync(TransferRequestDto dto) 
        {
            var httpContext = _httpContextAccessor.HttpContext!;
            var token = httpContext.Request.Headers["Authorization"].ToString();                   
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Missing Authorization token");

            var userIdClaim = httpContext.User?.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int tokenUserId))              
                throw new UnauthorizedAccessException("Invalid token");

            var client = _httpFactory.CreateClient("UserService");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var allAccountNumbers = dto.Transfers.Select(t => t.UserAccountNumber)
                                                 .Append(dto.MerchantAccountNumber)
                                                 .Distinct()
                                                 .ToList();

            var idMapResp = await client.PostAsJsonAsync("/api/users/account-to-id", allAccountNumbers);
            var accountMap = await idMapResp.Content.ReadFromJsonAsync<List<AccountNumberToIdDto>>();

            if (accountMap == null || accountMap.Count != allAccountNumbers.Count)
                throw new Exception("Some account numbers are invalid");

            var merchantId = accountMap.FirstOrDefault(m => m.AccountNumber == dto.MerchantAccountNumber)?.UserId;
            if (merchantId == null)
                throw new Exception("Merchant account not found");

            if (tokenUserId != merchantId.Value)
                throw new UnauthorizedAccessException("Token mismatch with merchant");

            var merchant = await client.GetFromJsonAsync<UserDto>($"/api/users/{merchantId}");
            if (merchant is null)
                throw new Exception("Merchant not found");

            decimal totalAmount = dto.Transfers.Sum(t => t.Amount);

            var transactionPerDay = _repo.GetTransactionNumberForUserAsSenderForToday(merchantId);
            if ((transactionPerDay >= 3)&&(merchant.Role=="User"))
                throw new Exception("Limit Reached for today");
            if ((totalAmount >= 5) && (merchant.Role == "User"))
                throw new Exception("Limit per transaction is 1000 rs");
            if (merchant.Balance < totalAmount)
                throw new Exception("Insufficient balance");

            var recipientIds = dto.Transfers.Select(t => accountMap
                                                 .First(a => a.AccountNumber == t.UserAccountNumber)
                                                 .UserId)
                                             .Distinct()
                                             .ToList();

            var resp = await client.PostAsJsonAsync("/api/users/bulk", recipientIds);
            var recipients = await resp.Content.ReadFromJsonAsync<List<UserDto>>();
            if (recipients == null || recipients.Count != recipientIds.Count)
                throw new Exception("Some recipient users not found");

            var userServiceTransferPayload = new
            {
                MerchantAccountNumber = dto.MerchantAccountNumber,
                Transfers = dto.Transfers
            };

            var txResp = await client.PostAsJsonAsync("/api/users/transfer", userServiceTransferPayload);
            if (!txResp.IsSuccessStatusCode)
                throw new Exception("Balance Update Faild on UserService");
            foreach (var transfer in dto.Transfers)
            {
                var userId = accountMap.First(x => x.AccountNumber == transfer.UserAccountNumber).UserId;
                var transaction = new Transaction
                {
                    FromUserId = merchantId.Value,
                    ToUserId = userId,
                    Amount = transfer.Amount,
                    Timestamp = DateTime.Now
                };
                await _repo.AddAsync(transaction);
            }

            await _repo.SaveChangesAsync();
        }
        public async Task<TransferRequestDto> ParseExcelAndBuildTransferDtoAsync(Stream stream, ClaimsPrincipal user)
        {
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.First();
            var rowCount = worksheet.Dimension.Rows;
            var colCount = worksheet.Dimension.Columns;

            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int col = 1; col <= colCount; col++)
            {
                var header = worksheet.Cells[1, col].Text.Trim();
                headers[header.ToLower()] = col;
            }

            if (!headers.ContainsKey("accountnumber") && !headers.ContainsKey("from"))
                throw new Exception("Missing 'From' column");

            if (!headers.ContainsKey("to"))
                throw new Exception("Missing 'To' column");

            if (!headers.ContainsKey("amount"))
                throw new Exception("Missing 'Amount' column");

            string merchantAccountNumber = worksheet.Cells[2, headers["from"]].Text.Trim();

            var transfers = new List<Models.TransferUserDto>();

            for (int row = 2; row <= rowCount; row++)
            {
                var toAccount = worksheet.Cells[row, headers["to"]].Text.Trim();
                var amountText = worksheet.Cells[row, headers["amount"]].Text.Trim();

                if (string.IsNullOrEmpty(toAccount) || string.IsNullOrEmpty(amountText))
                    continue;

                if (!decimal.TryParse(amountText, out var amount))
                    throw new Exception($"Invalid amount on row {row}");

                transfers.Add(new TransferUserDto
                {
                    UserAccountNumber = toAccount,
                    Amount = amount
                });
            }

            return new TransferRequestDto
            {
                MerchantAccountNumber = merchantAccountNumber,
                Transfers = transfers
            };
        }

    }
}
