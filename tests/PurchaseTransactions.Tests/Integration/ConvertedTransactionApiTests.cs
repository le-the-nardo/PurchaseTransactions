using System.Net;
using System.Net.Http.Json;
using PurchaseTransactions.Api.DTOs;
using PurchaseTransactions.Tests.Integration.Helpers;

namespace PurchaseTransactions.Tests.Integration;

public class ConvertedTransactionApiTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public ConvertedTransactionApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<TransactionResponse> CreateTransactionAsync(decimal amount = 100.00m, string date = "2024-06-15")
    {
        var request = new { Description = "Test purchase", TransactionDate = date, PurchaseAmount = amount };
        var response = await _client.PostAsJsonAsync("/transactions", request);
        return (await response.Content.ReadFromJsonAsync<TransactionResponse>())!;
    }

    [Fact]
    public async Task GetConverted_WithValidRateAvailable_Returns200WithConvertedAmount()
    {
        _factory.ExchangeRateService.Rate = 1.36m;
        var created = await CreateTransactionAsync(100.00m);

        var response = await _client.GetAsync($"/transactions/{created.Id}/converted?currency=Canada-Dollar");
        var body = await response.Content.ReadFromJsonAsync<ConvertedTransactionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(created.Id, body.Id);
        Assert.Equal("Test purchase", body.Description);
        Assert.Equal(100.00m, body.OriginalAmount);
        Assert.Equal(1.36m, body.ExchangeRate);
        Assert.Equal(136.00m, body.ConvertedAmount);
        Assert.Equal("Canada-Dollar", body.Currency);
    }

    [Fact]
    public async Task GetConverted_ConvertedAmountIsRoundedToTwoDecimalPlaces()
    {
        _factory.ExchangeRateService.Rate = 1.333m;
        var created = await CreateTransactionAsync(10.00m);

        var response = await _client.GetAsync($"/transactions/{created.Id}/converted?currency=Canada-Dollar");
        var body = await response.Content.ReadFromJsonAsync<ConvertedTransactionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(13.33m, body!.ConvertedAmount);
    }

    [Fact]
    public async Task GetConverted_WithUnknownTransactionId_Returns404()
    {
        _factory.ExchangeRateService.Rate = 1.36m;

        var response = await _client.GetAsync($"/transactions/{Guid.NewGuid()}/converted?currency=Canada-Dollar");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetConverted_WhenNoRateAvailable_Returns422WithErrorCode()
    {
        _factory.ExchangeRateService.Rate = null;
        var created = await CreateTransactionAsync();

        var response = await _client.GetAsync($"/transactions/{created.Id}/converted?currency=UnknownCurrency");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetailsWithErrorCode>();
        Assert.Equal("EXCHANGE_RATE_NOT_AVAILABLE", body!.ErrorCode?.ToString());
    }

    private record ProblemDetailsWithErrorCode(
        string? Title,
        string? Detail,
        System.Text.Json.JsonElement? ErrorCode);
}
