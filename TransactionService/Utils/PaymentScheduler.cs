using Hangfire;
using TransactionService.Models;
using TransactionService.Services;

namespace TransactionService.Utils
{
    public class PaymentScheduler : IPaymentScheduler
    {
        private readonly ITransactionService _transactionService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public PaymentScheduler(ITransactionService transactionService, IHttpContextAccessor httpContextAccessor)
        {
            _transactionService = transactionService;
            _httpContextAccessor = httpContextAccessor;
        }

        public void ScheduleBulkTransfers(TransferRequestDto dto)
        {
            for (int i = 0; i < dto.Transfers.Count; i++)
            {
                var singleTransfer = new TransferRequestDto
                {
                    MerchantAccountNumber = dto.MerchantAccountNumber,
                    Transfers = new List<TransferUserDto> { dto.Transfers[i] }
                };

                var delay = TimeSpan.FromSeconds(i * 20);

                BackgroundJob.Schedule(() =>
                    _transactionService.TransferAsync(singleTransfer),
                    delay
                );
            }
        }
    }

}
