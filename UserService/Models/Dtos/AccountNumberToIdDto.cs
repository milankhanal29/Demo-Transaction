namespace UserService.Models.Dtos
{
    public class AccountNumberToIdDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
