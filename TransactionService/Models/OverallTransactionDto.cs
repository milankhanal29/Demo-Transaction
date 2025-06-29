namespace TransactionService.Models
{
    public class OverallTransactionDto
    {
   
            public int Id { get; set; }
            public int FromUserId { get; set; }
            public int ToUserId { get; set; }
            public decimal Amount { get; set; }
            public DateTime Timestamp { get; set; }

            public string SenderName { get; set; } = string.Empty;
            public string SenderAccount { get; set; } = string.Empty;
            public string ReceiverName { get; set; } = string.Empty;
            public string ReceiverAccount { get; set; } = string.Empty;
        }
}
