using TransactionService.Models;

namespace TransactionService.Utils
{
    public interface IPaymentScheduler
    {
        void ScheduleBulkTransfers(TransferRequestDto dto);
    }

}
