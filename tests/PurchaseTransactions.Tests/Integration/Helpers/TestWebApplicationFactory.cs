using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PurchaseTransactions.Api.Data;
using PurchaseTransactions.Api.Services;

namespace PurchaseTransactions.Tests.Integration.Helpers;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

    public FakeExchangeRateService ExchangeRateService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={_dbPath}"));

            services.RemoveAll<IExchangeRateService>();
            services.AddScoped<IExchangeRateService>(_ => ExchangeRateService);
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Transactions.ExecuteDeleteAsync();
        ExchangeRateService.Rate = null;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}

public class FakeExchangeRateService : IExchangeRateService
{
    public decimal? Rate { get; set; }

    public Task<decimal?> GetExchangeRateAsync(string currency, DateOnly purchaseDate, CancellationToken ct = default)
        => Task.FromResult(Rate);
}
