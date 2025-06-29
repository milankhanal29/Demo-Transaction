namespace UserService.Models.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string AccountNumber { get; set; }
        public Role Role { get; set; }
        public decimal Balance { get; set; }
    }

}
