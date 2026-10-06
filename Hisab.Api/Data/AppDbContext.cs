using Hisab.Models;
using Microsoft.EntityFrameworkCore;

namespace Hisab.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Business> Businesses { get; set; }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Transaction> Transactions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // User → Business
      modelBuilder.Entity<User>()
          .HasOne(u => u.Business)
          .WithMany(b => b.Users)
          .HasForeignKey(u => u.BusinessId)
          .OnDelete(DeleteBehavior.Cascade);

      // Customer → Business
      modelBuilder.Entity<Customer>()
          .HasOne(c => c.Business)
          .WithMany()
          .HasForeignKey(c => c.BusinessId)
          .OnDelete(DeleteBehavior.Cascade);

      // Customer → Transaction
      modelBuilder.Entity<Transaction>()
          .HasOne(t => t.Customer)
          .WithMany()
          .HasForeignKey(t => t.CustomerId)
          .OnDelete(DeleteBehavior.Cascade);
    }
  }
  }
