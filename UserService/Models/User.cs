using System.ComponentModel.DataAnnotations;
using UserService.Models;

namespace UserService.Entity
{
    public class User
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        [Required]
        public string AccountNumber {  get; set; }
        public decimal Balance{ get; set; }
        public Role Role { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    }
}
