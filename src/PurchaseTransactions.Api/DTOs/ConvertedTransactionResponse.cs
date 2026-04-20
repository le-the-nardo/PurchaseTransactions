namespace PurchaseTransactions.Api.DTOs;

public record ConvertedTransactionResponse(
    Guid Id,
    string Description,
    DateOnly TransactionDate,
    decimal OriginalAmount,
    decimal ExchangeRate,
    decimal ConvertedAmount,
    string Currency);
