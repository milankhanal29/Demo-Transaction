using System.Security.Claims;
using TransactionService.Models;
using TransactionService.Utils;

namespace TransactionService.Services
{
    public interface ITransactionService
    {
        Task<PagedResult<OverallTransactionDto>> GetTransactionsForUserAsync(int userId, PaginationParams paginationParams);
        Task<List<TransactionDto>> GetAllTransactionsAsync();
        Task<List<object>> TransferAsync(TransferRequestDto dto);
        Task<TransferRequestDto> ParseExcelAndBuildTransferDtoAsync(Stream stream, ClaimsPrincipal user);

    }
}
