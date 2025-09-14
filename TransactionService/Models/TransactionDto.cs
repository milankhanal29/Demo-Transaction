namespace TransactionService.Models
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class TransactionMessageDto
    {
        public string MerchantAccountNumber { get; set; } = null!;
        public string UserAccountNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public int MerchantId { get; set; }
        public int RecipientId { get; set; }
    }
}
