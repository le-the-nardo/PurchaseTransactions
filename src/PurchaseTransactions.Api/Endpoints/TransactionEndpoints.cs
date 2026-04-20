using PurchaseTransactions.Api.Domain;
using PurchaseTransactions.Api.DTOs;
using PurchaseTransactions.Api.Repositories;
using PurchaseTransactions.Api.Services;

namespace PurchaseTransactions.Api.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("PurchaseTransactions.Api.Endpoints.Transactions");
        var group = app.MapGroup("/transactions");

        group.MapPost("/", async (
            CreateTransactionRequest request,
            ITransactionRepository repository,
            CancellationToken ct) =>
        {
            var transaction = Transaction.Create(request.Description, request.TransactionDate, request.PurchaseAmount);
            await repository.AddAsync(transaction, ct);

            logger.LogInformation("[TransactionEndpoints] Transaction created id={Id}, description={Description}, amount={Amount}, date={Date}",
                transaction.Id, transaction.Description, transaction.PurchaseAmount, transaction.TransactionDate);

            return Results.Created($"/transactions/{transaction.Id}", TransactionResponse.FromDomain(transaction));
        })
        .WithName("CreateTransaction");

        group.MapGet("/{id:guid}", async (
            Guid id,
            ITransactionRepository repository,
            CancellationToken ct) =>
        {
            var transaction = await repository.GetByIdAsync(id, ct);
            return transaction is null
                ? Results.NotFound()
                : Results.Ok(TransactionResponse.FromDomain(transaction));
        })
        .WithName("GetTransactionById");

        group.MapGet("/", async (
            ITransactionRepository repository,
            CancellationToken ct) =>
        {
            var transactions = await repository.GetAllAsync(ct);
            return Results.Ok(transactions.Select(TransactionResponse.FromDomain));
        })
        .WithName("GetAllTransactions");

        group.MapGet("/{id:guid}/converted", async (
            Guid id,
            string currency,
            ITransactionRepository repository,
            IExchangeRateService exchangeRateService,
            CancellationToken ct) =>
        {
            var transaction = await repository.GetByIdAsync(id, ct);
            if (transaction is null)
            {
                logger.LogWarning("[TransactionEndpoints] Conversion requested for unknown transaction id={Id}", id);
                return Results.NotFound();
            }

            var rate = await exchangeRateService.GetExchangeRateAsync(currency, transaction.TransactionDate, ct);
            if (rate is null)
                throw new DomainException(ErrorCodes.ExchangeRateNotAvailable,
                    $"No exchange rate available for '{currency}' within 6 months of the purchase date.");

            var converted = Math.Round(transaction.PurchaseAmount * rate.Value, 2, MidpointRounding.AwayFromZero);

            logger.LogInformation("[TransactionEndpoints] Conversion completed id={Id}, {OriginalAmount} USD → {ConvertedAmount} {Currency} at rate {Rate}",
                id, transaction.PurchaseAmount, converted, currency, rate.Value);

            return Results.Ok(new ConvertedTransactionResponse(
                transaction.Id,
                transaction.Description,
                transaction.TransactionDate,
                transaction.PurchaseAmount,
                rate.Value,
                converted,
                currency));
        })
        .WithName("GetConvertedTransaction");
    }
}
