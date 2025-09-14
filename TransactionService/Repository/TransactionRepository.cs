using Microsoft.EntityFrameworkCore;
using System.Data;
using TransactionService.Data;
using TransactionService.Models;
using TransactionService.Utils;

namespace TransactionService.Repository
{
    public class TransactionRepository:ITransactionRepository
    {
        private readonly AppDbContext _ctx;
        public TransactionRepository(AppDbContext ctx) => _ctx = ctx;

        public Task AddAsync(Transaction t) => _ctx.Transactions.AddAsync(t).AsTask();
        public int GetTransactionNumberForUserAsSenderForToday(int? merchantId)
            
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return _ctx.Transactions
                .Where(t => t.FromUserId == merchantId &&
                            t.Timestamp >= today &&
                            t.Timestamp < tomorrow)
                .Count();
        }


        public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
        public Task<List<Transaction>> GetByUserIdAsync(int userId) =>
            _ctx.Transactions.Where(t => t.FromUserId == userId).ToListAsync();

        public Task<List<Transaction>> GetALlTransactionAsync() =>
            _ctx.Transactions.ToListAsync();
        public async Task<PagedResult<OverallTransactionDto>> GetTransactionsWithNamesAsync(int userId, PaginationParams paginationParams)
        {
            var conn = _ctx.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "usp_GetTransactionsWithNames";
            cmd.CommandType = CommandType.StoredProcedure;

            var param = cmd.CreateParameter();
            param.ParameterName = "@UserId";
            param.Value = userId;
            cmd.Parameters.Add(param);

            var allResults = new List<OverallTransactionDto>();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                allResults.Add(new OverallTransactionDto
                {
                    Id = reader.GetInt32(0),
                    FromUserId = reader.GetInt32(1),
                    ToUserId = reader.GetInt32(2),
                    Amount = reader.GetDecimal(3),
                    Timestamp = reader.GetDateTime(4),
                    Status = reader.GetString(5),
                    Remark = reader.GetString(6),
                    SenderName = reader.GetString(7),
                    SenderAccount = reader.GetString(8),
                    ReceiverName = reader.GetString(9),
                    ReceiverAccount = reader.GetString(10)
                });
            }

            var count = allResults.Count;
            var items = allResults
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToList();

            return new PagedResult<OverallTransactionDto>(items, count, paginationParams.PageNumber, paginationParams.PageSize);
        }

    }
}
