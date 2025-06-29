using Microsoft.EntityFrameworkCore;
using UserService.Entity;
using UserService.Models;

namespace UserService.Data
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) :base(options)
        {  }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> refreshTokens { get; set; }

        protected override void OnModelCreating (ModelBuilder modelBuilder){
            modelBuilder.Entity<User>()
        .HasMany(u => u.RefreshTokens)
        .WithOne(t => t.User)
        .HasForeignKey(t => t.UserId);
            modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();


        }
    }
}
