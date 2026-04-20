using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PurchaseTransactions.Api.Data;
using PurchaseTransactions.Api.Domain;

namespace PurchaseTransactions.Api.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<TransactionRepository> _logger;

    public TransactionRepository(AppDbContext context, ILogger<TransactionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogDebug("[TransactionRepository] Fetching transaction by id={Id}", id);
        var sw = Stopwatch.StartNew();

        var transaction = await _context.Transactions.FindAsync(new object[] { id }, ct);

        sw.Stop();
        if (transaction is null)
            _logger.LogDebug("[TransactionRepository] Transaction not found id={Id} | elapsed={ElapsedMs}ms", id, sw.ElapsedMilliseconds);
        else
            _logger.LogDebug("[TransactionRepository] Transaction found id={Id} | elapsed={ElapsedMs}ms", id, sw.ElapsedMilliseconds);

        return transaction;
    }

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("[TransactionRepository] Fetching all transactions");
        var sw = Stopwatch.StartNew();

        var transactions = await _context.Transactions.AsNoTracking().ToListAsync(ct);

        sw.Stop();
        _logger.LogDebug("[TransactionRepository] Fetched {Count} transactions | elapsed={ElapsedMs}ms", transactions.Count, sw.ElapsedMilliseconds);

        return transactions;
    }

    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken ct = default)
    {
        _logger.LogDebug("[TransactionRepository] Persisting transaction id={Id}", transaction.Id);
        var sw = Stopwatch.StartNew();

        await _context.Transactions.AddAsync(transaction, ct);
        await _context.SaveChangesAsync(ct);

        sw.Stop();
        _logger.LogInformation("[TransactionRepository] Transaction persisted id={Id}, description={Description}, amount={Amount}, date={Date} | elapsed={ElapsedMs}ms",
            transaction.Id, transaction.Description, transaction.PurchaseAmount, transaction.TransactionDate, sw.ElapsedMilliseconds);

        return transaction;
    }
}
