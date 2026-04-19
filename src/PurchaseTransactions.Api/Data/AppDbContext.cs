using Microsoft.EntityFrameworkCore;
using PurchaseTransactions.Api.Domain;

namespace PurchaseTransactions.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(t => t.PurchaseAmount)
                .IsRequired()
                .HasColumnType("TEXT");

            entity.Property(t => t.TransactionDate)
                .IsRequired()
                .HasConversion(
                    v => v.ToString("yyyy-MM-dd"),
                    v => DateOnly.Parse(v));

            entity.ToTable("Transactions");
        });
    }
}
