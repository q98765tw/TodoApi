using Microsoft.EntityFrameworkCore;
using TodoApiGrokBot.Models;

namespace TodoApiGrokBot.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.PasswordHash).IsRequired();
        });
        b.Entity<TodoItem>()
            .HasOne(t => t.User).WithMany(u => u.Todos)
            .HasForeignKey(t => t.UserId).IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
