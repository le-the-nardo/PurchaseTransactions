namespace PurchaseTransactions.Api.Services;

public interface IExchangeRateService
{
    Task<decimal?> GetExchangeRateAsync(string currency, DateOnly purchaseDate, CancellationToken ct = default);
}
