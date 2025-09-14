using AutoMapper;
using OfficeOpenXml;
using SharedLibrary;
using System.Net.Http.Headers;
using System.Security.Claims;
using TransactionService.Models;
using TransactionService.Repository;
using TransactionService.Services.Kafka;
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
        private readonly IKafkaProducerService _kafkaProducer;

        public TransactionServiceImpl(ITransactionRepository repo,
                                  IHttpClientFactory httpFactory,
                                  IHttpContextAccessor httpContextAccessor,
                                  IKafkaProducerService kafkaProducer,
                                  IMapper mapper)
        {
            _repo = repo;
            _httpFactory = httpFactory;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _kafkaProducer = kafkaProducer; 

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
        //public async Task<List<object>> TransferAsync(TransferRequestDto dto)
        //{
        //    var httpContext = _httpContextAccessor.HttpContext!;
        //    var token = httpContext.Request.Headers["Authorization"].ToString();
        //    if (string.IsNullOrEmpty(token))
        //        throw new UnauthorizedAccessException("Missing Authorization token");

        //    var userIdClaim = httpContext.User?.FindFirst("userId");
        //    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int tokenUserId))
        //        throw new UnauthorizedAccessException("Invalid token");

        //    var client = _httpFactory.CreateClient("UserService");
        //    client.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

        //    var allAccountNumbers = dto.Transfers.Select(t => t.UserAccountNumber)
        //                                         .Append(dto.MerchantAccountNumber)
        //                                         .Distinct()
        //                                         .ToList();

        //    var idMapResp = await client.PostAsJsonAsync("/api/users/account-to-id", allAccountNumbers);
        //    var accountMap = await idMapResp.Content.ReadFromJsonAsync<List<AccountNumberToIdDto>>() ?? new();

        //    var merchantId = accountMap.FirstOrDefault(m => m.AccountNumber == dto.MerchantAccountNumber)?.UserId;
        //    if (merchantId == null)
        //        throw new Exception("Merchant account not found");

        //    if (tokenUserId != merchantId.Value)
        //        throw new UnauthorizedAccessException("Token mismatch with merchant");

        //    var merchant = await client.GetFromJsonAsync<UserDto>($"/api/users/{merchantId}");
        //    if (merchant is null)
        //        throw new Exception("Merchant not found");

        //    var transactionPerDay = _repo.GetTransactionNumberForUserAsSenderForToday(merchantId);
        //    if ((transactionPerDay >= 3) && merchant.Role == "User")
        //        throw new Exception("Limit Reached for today");

        //    var result = new List<object>();
        //    decimal currentBalance = merchant.Balance;

        //    foreach (var transfer in dto.Transfers)
        //    {
        //        string status = "Failed";
        //        string remark = "";
        //        int? recipientId = accountMap.FirstOrDefault(x => x.AccountNumber == transfer.UserAccountNumber)?.UserId;

        //        if (recipientId == null)
        //        {
        //            remark = "Invalid account number";
        //        }
        //        else if (transfer.Amount <= 0)
        //        {
        //            remark = "Invalid transfer amount";
        //        }
        //        else if (merchant.Role == "User" && transfer.Amount > 1000)
        //        {
        //            remark = "Amount exceeds per transaction limit";
        //        }
        //        else if (currentBalance < transfer.Amount)
        //        {
        //            remark = "Insufficient balance";
        //        }
        //        else
        //        {
        //            var userServicePayload = new
        //            {
        //                MerchantAccountNumber = dto.MerchantAccountNumber,
        //                Transfers = new List<TransferUserDto> { transfer }
        //            };

        //            var userServiceResp = await client.PostAsJsonAsync("/api/users/transfer", userServicePayload);

        //            if (!userServiceResp.IsSuccessStatusCode)
        //            {
        //                remark = "Balance update failed";
        //            }
        //            else
        //            {
        //                currentBalance -= transfer.Amount;
        //                status = "Success";
        //                remark = "Success";
        //            }
        //        }

        //        var tx = new Transaction
        //        {
        //            FromUserId = merchantId.Value,
        //            ToUserId = recipientId ?? 0,
        //            Amount = transfer.Amount,
        //            Timestamp = DateTime.Now,
        //            Status = status,
        //            Remark = remark
        //        };

        //        await _repo.AddAsync(tx);

        //        await _kafkaProducer.PublishTransactionAsync(new
        //        {
        //            tx.Id,
        //            tx.FromUserId,
        //            tx.ToUserId,
        //            tx.Amount,
        //            tx.Status,
        //            tx.Remark,
        //            tx.Timestamp
        //        });

        //        result.Add(new
        //        {
        //            AccountNumber = transfer.UserAccountNumber,
        //            Amount = transfer.Amount,
        //            Status = status,
        //            Remark = remark
        //        });
        //    }

        //    await _repo.SaveChangesAsync();
        //    return result;
        //}


        public async Task<List<object>> TransferAsync(TransferRequestDto dto)
        {
            var result = new List<object>();

            foreach (var transfer in dto.Transfers)
            {
                var kafkaMessage = new
                {
                    MerchantAccountNumber = dto.MerchantAccountNumber,
                    UserAccountNumber = transfer.UserAccountNumber,
                    Amount = transfer.Amount,
                    Timestamp = DateTime.UtcNow
                };

                await _kafkaProducer.PublishTransactionAsync(kafkaMessage);

                result.Add(new
                {
                    AccountNumber = transfer.UserAccountNumber,
                    Amount = transfer.Amount,
                    Status = "Queued",
                    Remark = "Transaction queued for processing"
                });
            }

            return result;
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
