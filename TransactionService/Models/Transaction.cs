namespace TransactionService.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int FromUserId { get; set; }  
        public int ToUserId { get; set; }   
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = "Pending"; 
        public string Remark { get; set; } = string.Empty;
    }

}
