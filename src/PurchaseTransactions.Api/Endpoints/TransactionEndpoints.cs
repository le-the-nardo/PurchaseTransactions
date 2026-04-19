using PurchaseTransactions.Api.Domain;
using PurchaseTransactions.Api.DTOs;
using PurchaseTransactions.Api.Repositories;

namespace PurchaseTransactions.Api.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/transactions");

        group.MapPost("/", async (CreateTransactionRequest request, ITransactionRepository repository, CancellationToken ct) =>
        {
            var transaction = Transaction.Create(request.Description, request.TransactionDate, request.PurchaseAmount);
            await repository.AddAsync(transaction, ct);
            var response = TransactionResponse.FromDomain(transaction);
            return Results.Created($"/transactions/{transaction.Id}", response);
        })
        .WithName("CreateTransaction");

        group.MapGet("/{id:guid}", async (Guid id, ITransactionRepository repository, CancellationToken ct) =>
        {
            var transaction = await repository.GetByIdAsync(id, ct);
            return transaction is null
                ? Results.NotFound()
                : Results.Ok(TransactionResponse.FromDomain(transaction));
        })
        .WithName("GetTransactionById");

        group.MapGet("/", async (ITransactionRepository repository, CancellationToken ct) =>
        {
            var transactions = await repository.GetAllAsync(ct);
            return Results.Ok(transactions.Select(TransactionResponse.FromDomain));
        })
        .WithName("GetAllTransactions");
    }
}
