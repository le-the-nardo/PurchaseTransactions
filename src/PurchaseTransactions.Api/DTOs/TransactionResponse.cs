using PurchaseTransactions.Api.Domain;

namespace PurchaseTransactions.Api.DTOs;

public record TransactionResponse(
    Guid Id,
    string Description,
    DateOnly TransactionDate,
    decimal PurchaseAmount
)
{
    public static TransactionResponse FromDomain(Transaction t) =>
        new(t.Id, t.Description, t.TransactionDate, t.PurchaseAmount);
}
