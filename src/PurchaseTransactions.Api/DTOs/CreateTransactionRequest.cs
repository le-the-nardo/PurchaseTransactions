namespace PurchaseTransactions.Api.DTOs;

public record CreateTransactionRequest(
    string? Description,
    DateOnly TransactionDate,
    decimal PurchaseAmount
);
