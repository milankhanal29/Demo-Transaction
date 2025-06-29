using TransactionService.Models;
using TransactionService.Utils;

namespace TransactionService.Repository
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction t);
        Task SaveChangesAsync();
        Task<List<Transaction>> GetByUserIdAsync(int merchantId);
        Task<List<Transaction>> GetALlTransactionAsync();
        Task<PagedResult<OverallTransactionDto>> GetTransactionsWithNamesAsync(int userId, PaginationParams paginationParams);
        int GetTransactionNumberForUserAsSenderForToday(int? merchantId);
    }
}
