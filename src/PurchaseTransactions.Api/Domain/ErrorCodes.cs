namespace PurchaseTransactions.Api.Domain;

public static class ErrorCodes
{
    public const string InvalidDescription = "INVALID_DESCRIPTION";
    public const string InvalidTransactionDate = "INVALID_TRANSACTION_DATE";
    public const string InvalidPurchaseAmount = "INVALID_PURCHASE_AMOUNT";
    public const string ExchangeRateNotAvailable = "EXCHANGE_RATE_NOT_AVAILABLE";
}
