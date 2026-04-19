using Microsoft.EntityFrameworkCore;
using PurchaseTransactions.Api.Data;
using PurchaseTransactions.Api.Domain;

namespace PurchaseTransactions.Api.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Transactions.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken ct = default)
        => await _context.Transactions.AsNoTracking().ToListAsync(ct);

    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken ct = default)
    {
        await _context.Transactions.AddAsync(transaction, ct);
        await _context.SaveChangesAsync(ct);
        return transaction;
    }
}
