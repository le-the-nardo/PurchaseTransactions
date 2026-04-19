using System.Net;
using System.Net.Http.Json;
using PurchaseTransactions.Api.DTOs;
using PurchaseTransactions.Tests.Integration.Helpers;

namespace PurchaseTransactions.Tests.Integration;

public class TransactionApiTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public TransactionApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Post_WithValidData_Returns201AndLocation()
    {
        var request = new { Description = "Grocery shopping", TransactionDate = "2024-06-15", PurchaseAmount = 85.50 };

        var response = await _client.PostAsJsonAsync("/transactions", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content.ReadFromJsonAsync<TransactionResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal("Grocery shopping", body.Description);
        Assert.Equal(85.50m, body.PurchaseAmount);
        Assert.Contains(body.Id.ToString(), response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Post_WithDescriptionOver50Chars_Returns422()
    {
        var request = new { Description = new string('x', 51), TransactionDate = "2024-06-15", PurchaseAmount = 10.00 };

        var response = await _client.PostAsJsonAsync("/transactions", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithNullDescription_Returns422()
    {
        var request = new { Description = (string?)null, TransactionDate = "2024-06-15", PurchaseAmount = 10.00 };

        var response = await _client.PostAsJsonAsync("/transactions", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithZeroAmount_Returns422()
    {
        var request = new { Description = "Test", TransactionDate = "2024-06-15", PurchaseAmount = 0.00 };

        var response = await _client.PostAsJsonAsync("/transactions", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithNegativeAmount_Returns422()
    {
        var request = new { Description = "Test", TransactionDate = "2024-06-15", PurchaseAmount = -5.00 };

        var response = await _client.PostAsJsonAsync("/transactions", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Post_AmountIsRoundedToNearestCent()
    {
        var request = new { Description = "Rounded amount", TransactionDate = "2024-06-15", PurchaseAmount = 10.555 };

        var response = await _client.PostAsJsonAsync("/transactions", request);
        var body = await response.Content.ReadFromJsonAsync<TransactionResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(10.56m, body!.PurchaseAmount);
    }

    [Fact]
    public async Task GetById_AfterCreation_Returns200WithCorrectData()
    {
        var request = new { Description = "Lunch", TransactionDate = "2024-06-15", PurchaseAmount = 12.99 };
        var postResponse = await _client.PostAsJsonAsync("/transactions", request);
        var created = await postResponse.Content.ReadFromJsonAsync<TransactionResponse>();

        var getResponse = await _client.GetAsync($"/transactions/{created!.Id}");
        var body = await getResponse.Content.ReadFromJsonAsync<TransactionResponse>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(created.Id, body!.Id);
        Assert.Equal("Lunch", body.Description);
        Assert.Equal(12.99m, body.PurchaseAmount);
    }

    [Fact]
    public async Task GetById_WithUnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/transactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AfterMultipleCreations_ReturnsAllTransactions()
    {
        var requests = new[]
        {
            new { Description = "Item A", TransactionDate = "2024-06-15", PurchaseAmount = 10.00 },
            new { Description = "Item B", TransactionDate = "2024-06-16", PurchaseAmount = 20.00 },
            new { Description = "Item C", TransactionDate = "2024-06-17", PurchaseAmount = 30.00 }
        };

        foreach (var req in requests)
            await _client.PostAsJsonAsync("/transactions", req);

        var response = await _client.GetAsync("/transactions");
        var body = await response.Content.ReadFromJsonAsync<List<TransactionResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, body!.Count);
    }
}
