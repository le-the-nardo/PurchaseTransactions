using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PurchaseTransactions.Api.Swagger;
using PurchaseTransactions.Api.Data;
using PurchaseTransactions.Api.Endpoints;
using PurchaseTransactions.Api.Middleware;
using PurchaseTransactions.Api.Repositories;
using PurchaseTransactions.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddHttpClient("TreasuryApi", c =>
    c.BaseAddress = new Uri("https://api.fiscaldata.treasury.gov"));
builder.Services.AddScoped<IExchangeRateService, TreasuryExchangeRateService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.OperationFilter<CurrencyExamplesOperationFilter>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Starting PurchaseTransactions API | environment={Environment}", app.Environment.EnvironmentName);

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        var url = app.Urls.FirstOrDefault() ?? "http://localhost:5039";
        Process.Start(new ProcessStartInfo($"{url}/swagger") { UseShellExecute = true });
    });
}

app.MapTransactionEndpoints();

app.Run();

public partial class Program { }
