using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PurchaseTransactions.Api.Services;

public class TreasuryExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TreasuryExchangeRateService> _logger;

    public TreasuryExchangeRateService(IHttpClientFactory httpClientFactory, ILogger<TreasuryExchangeRateService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("TreasuryApi");
        _logger = logger;
    }

    public async Task<decimal?> GetExchangeRateAsync(string currency, DateOnly purchaseDate, CancellationToken ct = default)
    {
        var sixMonthsAgo = purchaseDate.AddMonths(-6);
        var url = $"/services/api/fiscal_service/v1/accounting/od/rates_of_exchange" +
                  $"?fields=country_currency_desc,exchange_rate,record_date" +
                  $"&filter=country_currency_desc:eq:{Uri.EscapeDataString(currency)},record_date:lte:{purchaseDate:yyyy-MM-dd},record_date:gte:{sixMonthsAgo:yyyy-MM-dd}" +
                  $"&sort=-record_date&page%5Bsize%5D=1";

        _logger.LogInformation("[TreasuryExchangeRateService] Requesting exchange rate currency={Currency}, window={From} to {To}",
            currency, sixMonthsAgo, purchaseDate);
        _logger.LogDebug("[TreasuryExchangeRateService] Treasury API URL: {Url}", url);

        var sw = Stopwatch.StartNew();
        var httpResponse = await _httpClient.GetAsync(url, ct);
        sw.Stop();

        var responseBody = await httpResponse.Content.ReadAsStringAsync(ct);

        _logger.LogInformation("[TreasuryExchangeRateService] Treasury API responded {StatusCode} in {ElapsedMs}ms | currency={Currency}",
            (int)httpResponse.StatusCode, sw.ElapsedMilliseconds, currency);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("[TreasuryExchangeRateService] Treasury API error {StatusCode} for currency={Currency} | body={Body}",
                (int)httpResponse.StatusCode, currency, responseBody);
            throw new HttpRequestException(
                $"Treasury API returned {(int)httpResponse.StatusCode} for currency '{currency}'.");
        }

        _logger.LogDebug("[TreasuryExchangeRateService] Treasury API response body: {Body}", responseBody);

        var response = JsonSerializer.Deserialize<TreasuryApiResponse>(responseBody);
        var entry = response?.Data?.FirstOrDefault();

        if (entry is null)
        {
            _logger.LogWarning("[TreasuryExchangeRateService] No exchange rate found for currency={Currency} within 6 months of {PurchaseDate}",
                currency, purchaseDate);
            return null;
        }

        if (!decimal.TryParse(entry.ExchangeRate, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate))
        {
            _logger.LogError("[TreasuryExchangeRateService] Failed to parse exchange rate '{RateValue}' for currency={Currency}",
                entry.ExchangeRate, currency);
            return null;
        }

        _logger.LogInformation("[TreasuryExchangeRateService] Exchange rate resolved currency={Currency}, rate={Rate}, recordDate={RecordDate}",
            currency, rate, entry.RecordDate);

        return rate;
    }

    private record TreasuryApiResponse(
        [property: JsonPropertyName("data")] List<TreasuryRateEntry>? Data);

    private record TreasuryRateEntry(
        [property: JsonPropertyName("exchange_rate")] string? ExchangeRate,
        [property: JsonPropertyName("record_date")] string? RecordDate);
}
